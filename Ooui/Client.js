// Ooui v1.0.0

var debug = false;

let nodes = {};
let hasText = {};

let socket = null;
let wasmSend = null;

let lastRootElementPath = "";

function send (json) {
    if (debug) console.log ("Send", json);
    if (socket != null) {
        socket.send (json);
    }
    else if (wasmSend != null) {
        wasmSend (json);
    }
}

const mouseEvents = {
    click: true,
    dblclick: true,
    mousedown: true,
    mouseenter: true,
    mouseleave: true,
    mousemove: true,
    mouseout: true,
    mouseover: true,
    mouseup: true,
    wheel: true,
};

const inputEvents = {
    input: true,
    change: true,
    keyup: true,
};

const elementEvents = {
    load: true
}

function getSize () {
    return {
        height: window.innerHeight,
        width: window.innerWidth
    };
}

function setCookie (name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date ();
        date.setTime(date.getTime () + (days*24*60*60*1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "")  + expires + "; path=/";
}

function saveSize (s) {
    setCookie ("oouiWindowWidth", s.width, 7);
    setCookie ("oouiWindowHeight", s.height, 7);
}

function initializeNavigation() {
    monitorHashChanged();
    const em = {
        m: "event",
        id: "window",
        k: "hashchange",
        v: window.location
    };
    saveSize(em.v);
    const ems = JSON.stringify(em);
    send(ems);
    if (debug) console.log("Event", em);
}

let isWindowLoaded = false;
window.addEventListener("load", function () {
    connectWebSocket();
    isWindowLoaded = true;
});

// Main entrypoint
function ooui(rootElementPath) {
    lastRootElementPath = rootElementPath;
    if (isWindowLoaded) {
        connectWebSocket();
    }
}

let reloadTryCount = 0;
let reloadRequestTime = 0;
function reloadSocket() {
    const now = (new Date()).getTime ();
    if (now - reloadRequestTime > 0) {
        reloadRequestTime = now + 100;
        reloadTryCount++;
        window.setTimeout(function () {
            connectWebSocket();
        }, 10);
    }
}

function connectWebSocket() {
    rootElementPath = lastRootElementPath;
    if (rootElementPath.length === 0)
        return;
    // In WebAssembly mode the page already talks to the runtime, there is no server to connect to.
    if (wasmSend != null)
        return;

    console.log("Initializing Ooui web socket");

    if (reloadTryCount > 0) {
        let $body = getBodyNode();
        while ($body.firstChild)
            $body.removeChild($body.lastChild);
        nodes = {};
        hasText = {};
    }

    var initialSize = getSize ();
    saveSize (initialSize);

    var wsArgs = (rootElementPath.indexOf("?") >= 0 ? "&" : "?") +
        "w=" + initialSize.width + "&h=" + initialSize.height;

    var proto = "ws";
    if (location.protocol == "https:") {
        proto = "wss";
    }

    socket = new WebSocket (proto + "://" + document.location.host + rootElementPath + wsArgs, "ooui");

    let socketOpened = false;

    socket.addEventListener ("open", function (event) {
        console.log("Web socket opened");
        socketOpened = true;
        initializeNavigation();
    });

    socket.addEventListener ("error", function (event) {
        console.error ("Web socket error", event);
    });

    socket.addEventListener ("close", function (event) {
        //console.error("Web socket close", event);
        if (socketOpened) {
            reloadSocket();
        }
    });

    socket.addEventListener("message", function (event) {
        const messages = JSON.parse (event.data);
        if (debug) console.log("Messages", messages);
        if (Array.isArray (messages)) {
            messages.forEach (function (m) {
                // console.log('Raw value from server', m.v);
                m.v = fixupValue (m.v);
                processMessage (m);
            });
        }
    });

    console.log("Web socket created");

    monitorSizeChanges (1000/10);
}

function monitorHashChanged() {
    function hashChangeHandler() {
        const em = {
            m: "event",
            id: "window",
            k: "hashchange",
            v: window.location
        };
        saveSize(em.v);
        const ems = JSON.stringify(em);
        send(ems);
        if (debug) console.log("Event", em);
    }

    window.addEventListener("hashchange", hashChangeHandler, false);
}

function monitorSizeChanges (millis)
{
    var resizeTimeout;
    function resizeThrottler() {
        if (!resizeTimeout) {
            resizeTimeout = setTimeout(function() {
                resizeTimeout = null;
                resizeHandler();            
            }, millis);
        }
    }

    function resizeHandler() {
        const em = {
            m: "event",
            id: "window",
            k: "resize",
            v: getSize (),
        };
        saveSize (em.v);
        const ems = JSON.stringify (em);
        send (ems);
        if (debug) console.log ("Event", em);
    }

    window.addEventListener("resize", resizeThrottler, false);
}

const keyboardEvents = {
    keydown: true,
    keypress: true,
};

const scrollEvents = {
    scroll: true,
};

function getBodyNode() {
    const bodyNode = document.getElementById("ooui-body");
    return bodyNode || document.body;
}

function getNode (id) {
    switch (id) {
        case "window": return window;
        case "document": return document;
        case "document.body": return getBodyNode();
        default: return nodes[id];
    }
}

function getOrCreateElement (id, tagName) {
    var e = document.getElementById (id);
    if (e) {
        if (e.firstChild && e.firstChild.nodeType == Node.TEXT_NODE)
            hasText[e.id] = true;
        return e;
    }
    return document.createElement (tagName);
}

function msgCreate (m) {
    const id = m.id;
    const tagName = m.k;
    const node = tagName === "#text" ?
        document.createTextNode ("") :
        getOrCreateElement (id, tagName);
    if (tagName !== "#text")
        node.id = id;
    nodes[id] = node;
    if (debug) console.log ("Created node", node);
}

function msgSet (m) {
    const id = m.id;
    const node = getNode (id);
    if (!node) {
        console.error ("Unknown node id", m);
        return;
    }
    const parts = m.k.split(".");
    let o = node;
    for (let i = 0; i < parts.length - 1; i++) {
        o = o[parts[i]];
    }
    const lastPart = parts[parts.length - 1];
    const value = lastPart === "htmlFor" ? m.v.id : m.v;
    o[lastPart] = value;
    if (debug) console.log ("Set", node, parts, value);
}

function msgSetAttr (m) {
    const id = m.id;
    const node = getNode (id);
    if (!node) {
        console.error ("Unknown node id", m);
        return;
    }
    node.setAttribute(m.k, m.v);
    if (m.k === "value" && ("value" in node)) {
        node.value = m.v;
    }
    else if (m.k === "checked" && ("checked" in node)) {
        node.checked = true;
    }
    if (debug) console.log ("SetAttr", node, m.k, m.v);
}

function msgRemAttr (m) {
    const id = m.id;
    const node = getNode (id);
    if (!node) {
        console.error ("Unknown node id", m);
        return;
    }
    node.removeAttribute(m.k);
    if (m.k === "checked" && ("checked" in node)) {
        node.checked = false;
    }
    if (debug) console.log ("RemAttr", node, m.k);
}

function getCallerProperty(target, accessorStr) {
    const arr = accessorStr.split('.');
    var caller = target; 
    var property = target;
    arr.forEach(function (v) {
        caller = property;
        property = caller[v];
    });
    return [caller, property];
}

function msgCall (m) {
    const id = m.id;
    const node = getNode (id);
    if (!node) {
        console.error ("Unknown node id", m);
        return;
    }
    const target = node;
    if (m.k === "insertBefore" && m.v[0].nodeType == Node.TEXT_NODE && m.v[1] == null && hasText[id]) {
        // Text is already set so it clear it first
        if (target.firstChild)
            target.removeChild (target.firstChild);
        delete hasText[id];
    }
    //const f = target[m.k];
    const f = getCallerProperty(target, m.k);
    if (debug) console.log ("Call", node, f, m.v);
    const r = f[1].apply (f[0], m.v);
    if (typeof m.rid === 'string' || m.rid instanceof String) {
        nodes[m.rid] = r;
    }
}

function msgListen (m) {
    const node = getNode (m.id);
    if (!node) {
        console.error ("Unknown node id", m);
        return;
    }
    if (debug) console.log ("Listen", node, m.k);
    node.addEventListener(m.k, function (e) {
        const em = {
            m: "event",
            id: m.id,
            k: m.k,
        };
        if (inputEvents[m.k]) {
            em.v = (node.tagName === "INPUT" && (node.type === "checkbox" || node.type === "radio")) ?
                node.checked :
                node.value;
        }
        else if (mouseEvents[m.k]) {
            em.v = {
                offsetX: e.offsetX,
                offsetY: e.offsetY,
            };
        }
        else if (keyboardEvents[m.k]) {
            em.v = e.key;
        }
        else if (scrollEvents[m.k]) {
            em.v = {
                scrollLeft: node.scrollLeft,
                scrollTop: node.scrollTop,
                scrollWidth: node.scrollWidth,
                scrollHeight: node.scrollHeight,
                clientWidth: node.clientWidth,
                clientHeight: node.clientHeight
            };
        }
        else if (elementEvents[m.k]) {
            em.v = {
                clientHeight: node.clientHeight,
                clientWidth: node.clientWidth
            }
        }
        const ems = JSON.stringify (em);
        send (ems);
        if (debug) console.log ("Event", em);
        if (em.k === "submit")
            e.preventDefault ();
    });
}

function processMessage (m) {
    switch (m.m) {
        case "nop":
            break;
        case "create":
            msgCreate (m);
            break;
        case "set":
            msgSet (m);
            break;
        case "setAttr":
            msgSetAttr (m);
            break;
        case "remAttr":
            msgRemAttr (m);
            break;
        case "call":
            msgCall (m);
            break;
        case "listen":
            msgListen (m);
            break;
        default:
            console.error ("Unknown message type", m.m, m);
    }
}

function fixupValue (v) {
    var x, n;
    if (Array.isArray (v)) {
        for (x in v) {
            v[x] = fixupValue (v[x]);
        }
        return v;
    }
    else if (typeof v === 'string' || v instanceof String) {
        if ((v.length > 1) && (v[0] === "\u2999")) {
            // console.log("V", v);
            return getNode (v);
        }
    }
    else if (!!v && v.hasOwnProperty("id") && v.hasOwnProperty("k")) {
        return fixupValue(v["id"])[v["k"]];
    }
    return v;
}


// == WebAssembly Support ==
//
// When Ooui runs entirely in the browser (net11.0-browser / dotnet.native.wasm) there is no
// web socket. The .NET side pushes serialized messages through the JSImport binding
// "globalThis.oouiWasm.receiveMessages", and the page pushes user input back through the
// [JSExport] methods handed to oouiWasm.start.

const oouiWasm = {
    sessionId: null,
    exports: null,
    started: false,

    // Called by the host page once the .NET runtime is up and the exports are available.
    // exports must provide StartSession(sessionId, path, size) and ReceiveMessageJson(sessionId, json).
    start (exports, sessionId) {
        if (this.started)
            return;
        this.exports = exports;
        this.sessionId = sessionId || "main";

        // Register the receive path before starting the session so that the very first
        // messages (document.body appendChild) can already be delivered.
        this.started = true;
        wasmSend = function (json) {
            exports.ReceiveMessageJson (oouiWasm.sessionId, json);
        };

        const size = getSize ();
        saveSize (size);
        lastRootElementPath = "/";
        exports.StartSession (this.sessionId, "/", Math.round (size.width) + " " + Math.round (size.height));

        initializeNavigation ();
        monitorSizeChanges (1000 / 30);

        const loading = document.getElementById ("loading");
        if (loading)
            loading.hidden = true;
    },

    // Called from .NET: receives a JSON array of messages created by the server side session.
    receiveMessages (sessionId, json) {
        if (debug) console.log ("WebAssembly Receive", json);
        if (!json)
            return;
        const messages = JSON.parse (json);
        for (let i = 0; i < messages.length; i++) {
            const m = messages[i];
            m.v = fixupValue (m.v);
            processMessage (m);
        }
    },
};

globalThis.oouiWasm = oouiWasm;
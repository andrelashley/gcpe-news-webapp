// <!-- Snowplow starts plowing - Standalone Search vE.2.14.0 -->
; (function (p, l, o, w, i, n, g) {
    if (!p[i]) {
    p.GlobalSnowplowNamespace = p.GlobalSnowplowNamespace || [];
        p.GlobalSnowplowNamespace.push(i); p[i] = function () {
            (p[i].q = p[i].q || []).push(arguments)
        }; p[i].q = p[i].q || []; n = l.createElement(o); g = l.getElementsByTagName(o)[0]; n.async = 1;
        n.src = w; g.parentNode.insertBefore(n, g)
    }
}(window, document, "script", "https://www2.gov.bc.ca/StaticWebResources/static/sp/sp-2-14-0.js", "snowplow"));
var collector = 'spt.apps.gov.bc.ca';
window.snowplow('newTracker', 'rt', collector, {
    appId: "Snowplow_standalone",
    cookieLifetime: 86400 * 548,
    platform: 'web',
    post: true,
    forceSecureTracker: true,
    contexts: {
        webPage: true,
        performanceTiming: true
    }
});
window.snowplow('enableActivityTracking', 30, 30); // Ping every 30 seconds after 30 seconds
window.snowplow('enableLinkClickTracking');
window.snowplow('trackPageView');
// Change the 'Search term parameter' to what is being used on the site, generally it is q.
var searchParameter = 'q'
// This checks for the search parameter that was put in above along with an equal, for example q= or s=
if (window.location.search.indexOf(searchParameter + '=') > -1) {
    window.snowplow('trackSiteSearch',
        getUrlParamArray(searchParameter, '')
    );
}

function getUrlParamArray(param, defaultValue) {
    var vars = [];
    var parts = window.location.href.replace(/[?&]+([^=&]+)=([^&]*)/gi, function (m, key, value) {
        if (key === param) {
            vars.push(value);
        }
    });
    return vars;
}
//<!-- Snowplow stop plowing -->
mergeInto(LibraryManager.library, {
  CDPanelAttach: function(name) {
    if (!window.CursorDashTools || !window.cursorDashUnity) return 0;
    window.CursorDashTools.attach(UTF8ToString(name), window.cursorDashUnity); return 1;
  },
  CDPanelReceive: function(json) { if(window.CursorDashTools) window.CursorDashTools.receive(JSON.parse(UTF8ToString(json))); },
  CDStoreGet: function(key) {
    try { window.cdStoreError='';var v=localStorage.getItem(UTF8ToString(key)); if(v===null)return 0;var p=_malloc(lengthBytesUTF8(v)+1);stringToUTF8(v,p,lengthBytesUTF8(v)+1);return p; }
    catch(e){window.cdStoreError=String(e);return 0;}
  },
  CDStoreSave: function(key,value) {
    try {var k=UTF8ToString(key);if(['CursorDash.v2.user','CursorDash.v2.presets','CursorDash.v2.tutorial'].indexOf(k)<0)throw new Error('허용되지 않은 key');localStorage.setItem(k,UTF8ToString(value));window.cdStoreError='';return 1;}
    catch(e){window.cdStoreError='브라우저 저장 실패: '+String(e);return 0;}
  },
  CDStoreError: function(){var s=window.cdStoreError||'';var p=_malloc(lengthBytesUTF8(s)+1);stringToUTF8(s,p,lengthBytesUTF8(s)+1);return p;},
  CDStoreFree: function(ptr){_free(ptr);}
});

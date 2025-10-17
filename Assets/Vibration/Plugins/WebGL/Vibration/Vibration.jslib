mergeInto(LibraryManager.library, {

    _HasVibrator: function () {
        return (typeof navigator.vibrate === "function");
    },
    
    _Vibrate: function (duration) {
        navigator.vibrate(duration);
    }

});

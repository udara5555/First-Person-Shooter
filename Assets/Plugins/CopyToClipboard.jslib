mergeInto(LibraryManager.library, {
    CopyToClipboardJS: function(text) {
        var str = UTF8ToString(text);
        
        if (navigator.clipboard && navigator.clipboard.writeText) {
            // Modern approach using Clipboard API
            navigator.clipboard.writeText(str).catch(function(err) {
                console.error("Failed to copy to clipboard: ", err);
            });
        } else {
            // Fallback for older browsers
            var textarea = document.createElement("textarea");
            textarea.value = str;
            textarea.style.position = "fixed";
            textarea.style.left = "-9999px";
            document.body.appendChild(textarea);
            textarea.select();
            try {
                document.execCommand("copy");
                console.log("Room ID copied to clipboard: " + str);
            } catch (err) {
                console.error("Failed to copy: ", err);
            }
            document.body.removeChild(textarea);
        }
    }
});
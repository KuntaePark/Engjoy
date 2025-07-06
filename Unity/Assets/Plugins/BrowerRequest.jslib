var LibraryBrowserRequest = {
    $BrowserRequest: {},
    
    SendRequest: function(methodPtr, urlPtr, bodyJsonPtr, requestId, callbackPtr) {
        const method = UTF8ToString(methodPtr);
        const url = UTF8ToString(urlPtr);
        const bodyJson = UTF8ToString(bodyJsonPtr);

        //add headers
        //assume csrf token is always included
        var token = document.querySelector('meta[name="_csrf"]')
                    .getAttribute('content');
        var header = document.querySelector('meta[name="_csrf_header"]')
                    .getAttribute('content');

        const headers = {
            'Content-Type' : 'application/json',
            [header] : token
        }
        const options = {
            method: method,
            headers: headers,
        }

        if(method !== 'GET' && method !== 'HEAD') {
            options.body = bodyJson;
        }

        fetch(url, options)
        .then(response => {
            return response.text().then(body => {
                const result = JSON.stringify({
                    status: response.status,
                    body: body
                });

                const buffer = allocate(intArrayFromString(result), 'i8', ALLOC_NORMAL);
                getWasmTableEntry(callbackPtr)(buffer, requestId);
            });
        })
        .catch(error => {
            const result = JSON.stringify({
                status: 0,
                body: error.toString()
            });
            const buffer = allocate(intArrayFromString(result), 'i8', ALLOC_NORMAL);
            getWasmTableEntry(callbackPtr)(buffer, requestId);
        });
    },

    FreeBuffer: function(ptr) {
        _free(ptr);
    },
}

autoAddDeps(LibraryBrowserRequest, '$BrowserRequest');
mergeInto(LibraryManager.library, LibraryBrowserRequest);
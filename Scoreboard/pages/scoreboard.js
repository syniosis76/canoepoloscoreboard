function asyncGetRequest(url, responseFunction) {
    xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
            var response = JSON.parse(xmlHttp.responseText);
            responseFunction(response);
        }
    };
    xmlHttp.open("GET", url, true);
    xmlHttp.send();
}

function makeRequest (method, url, postData = "")
{
    return new Promise(function (resolve, reject)
    {
        var xhr = new XMLHttpRequest();
        xhr.open(method, url);
        xhr.onload = function ()
        {
            if (this.status >= 200 && this.status < 300)
            {
                var response = JSON.parse(xhr.responseText);
                resolve({ type: "url", path: url, status: this.status, response: response });
            }
            else
            {
                resolve({ type: "url", path: url, status: this.status, response: xhr.statusText });
            }
        };
        xhr.onerror = function ()
        {
            resolve({ type: "url", path: url, status: this.status > 0 ? this.status : 500 , response: xhr.statusText });
        };
        try
        {
            xhr.send(postData);
        }
        catch (error)
        {
            resolve({ type: "url", path: url, status: 500, response: error.message });
        }
    });
}

function executeMethod(method)
{
    xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", method, true);
    xmlHttp.send();            
}

function ordinalSuffix(i)
{
    var j = i % 10,
        k = i % 100;
    if (j == 1 && k != 11) {
        return i + "st";
    }
    if (j == 2 && k != 12) {
        return i + "nd";
    }
    if (j == 3 && k != 13) {
        return i + "rd";
    }
    return i + "th";
}

function saveToFile(filename, type, data) {
    var blob = new Blob([data], {type: type});
    if(window.navigator.msSaveOrOpenBlob) {
        window.navigator.msSaveBlob(blob, filename);
    }
    else
    {
        var elem = window.document.createElement('a');
        elem.href = window.URL.createObjectURL(blob);
        elem.download = filename;        
        document.body.appendChild(elem);
        elem.click();        
        document.body.removeChild(elem);
        window.URL.revokeObjectURL(blob);
    }
}

function copyToClipboard(text) {
    if (window.clipboardData && window.clipboardData.setData) {
        // IE specific code path to prevent textarea being shown while dialog is visible.
        return clipboardData.setData("Text", text); 

    } else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
        var textarea = document.createElement("textarea");
        textarea.textContent = text;
        textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in MS Edge.
        document.body.appendChild(textarea);
        textarea.select();
        try {
            return document.execCommand("copy");  // Security exception may be thrown by some browsers.
        } catch (ex) {
            console.warn("Copy to clipboard failed.", ex);
            return false;
        } finally {
            document.body.removeChild(textarea);
        }
    }
}

class DataSources
{      
    constructor()
    {
        this.dataSources = [];
        this.restoreDataSources();               
    }

    getDataSource(path)
    {
        if (path.endsWith("/results"))
        {
            path = path.substring(0, path.length - 7);

            
        }

        return this.dataSources.find(function (findItem) { return findItem.path === path; });  
    }

    setDataSource(dataSource)
    {
        var existingDataSource = this.getDataSource(dataSource.path);
        if (existingDataSource != undefined)
        {
            Object.assign(existingDataSource, dataSource)
        }
        else
        {
            this.dataSources.push(dataSource)
        }
    }

    saveDataSource(path)
    {
        var dataSource = this.getDataSource(path);
        if (dataSource != undefined)
        {
            saveToFile("results.json", "application/json", JSON.stringify(dataSource.response));
        }
    }

    removeDataSource(path)
    {
        var dataSource = this.getDataSource(path);
        if (dataSource != undefined)
        {
            this.dataSources.splice(this.dataSources.indexOf(dataSource), 1);
            results.displayResults();
        }
    }

    storeDataSources()
    {
        var dataSourcesString = JSON.stringify(this.dataSources);
        localStorage.dataSources = dataSourcesString;
    }

    restoreDataSources()
    {
        var dataSourcesString = localStorage.dataSources;                

        if (dataSourcesString != undefined)
        {
            var dataSources = JSON.parse(dataSourcesString);
            this.dataSources = dataSources
        }             
    }
    
    handleFiles(files)
    {
        var _this = this;
        if (files.length >= 1)
        {
            var file = files[0];
            var fileReader = new FileReader();                
            fileReader.onload = function(e)
            {
                var resultsString = fileReader.result;                        
                _this.setDataSource({ type: "file", path: file.name, status: 200, response: JSON.parse(resultsString) });
                results.displayResults();
            }
            fileReader.readAsText(file);
        }
        document.getElementById("file-selector").value = "";
    }

    enterUrl()
    {
        var url = prompt("Enter URL");
        if (url != null)
        {
            this.setDataSource({ type: "url", path: url })
            results.displayResults();
        }
    }            

    getDataSourcesTable()
    {
        var resultsContent = "<tbody>";

        for (var dataSourceIndex = 0; dataSourceIndex < this.dataSources.length; dataSourceIndex++)
        {                    
            var dataSource = this.dataSources[dataSourceIndex];
            resultsContent += "<tr>"
                + "<td>" + dataSource.path + "</td>"
                + "<td>" + (dataSource.status != undefined ? dataSource.status : 0) + "</td>"
                + "<td><button onclick=\"results.dataSources.saveDataSource('" + dataSource.path + "')\">Save</button></td>"
                + "<td><button onclick=\"results.dataSources.removeDataSource('" + dataSource.path + "')\">Remove</button></td>"                
                + "<tr>";      
        }                    

        resultsContent += "</tbody>";

        return resultsContent;
    }
        
    readDataSources(resultsFunction)
    {
        var _this = this;

        var promises = this.dataSources.map(function (dataSource)
        {
            if (dataSource.type === "url") return makeRequest("GET", dataSource.path + "results");
            else return null;
        });
        Promise.all(promises)
        .then(function(allResults)
        {                                        
            for (var resultIndex = 0; resultIndex < allResults.length; resultIndex++)
            {                    
                var result = allResults[resultIndex];
                if (result != null)
                {
                    var dataSource = _this.getDataSource(result.path);
                    if (dataSource)
                    {
                        dataSource.status = result.status;
                        if (result.status == 200)
                        {                            
                            dataSource.response = result.response;
                        }
                    }
                }
            }

            _this.storeDataSources(); 

            resultsFunction();   
        })
        .catch(e => { console.log(e); });     
    }    
}            

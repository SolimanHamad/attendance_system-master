function downloadCSV(csv, filename) {
    var csvFile;
    var downloadLink;

    // CSV file
    csvFile = new Blob([csv], { type: "text/csv" });

    // Download link
    downloadLink = document.createElement("a");

    // File name
    downloadLink.download = filename;

    // Create a link to the file
    downloadLink.href = window.URL.createObjectURL(csvFile);

    // Hide download link
    downloadLink.style.display = "none";

    // Add the link to DOM
    document.body.appendChild(downloadLink);

    // Click download link
    downloadLink.click();
}

function exportTableToCSV(filename) {
    var csv = [];
    var rows = document.querySelectorAll("table tr");

    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll("td, th");

        for (var j = 0; j < cols.length; j++)
            row.push(cols[j].innerText);

        csv.push(row.join(","));
    }

    // Download CSV file
    downloadCSV(csv.join("\n"), filename);
}

function exportTableToExcel(tableID, filename = 'excel-file', sheetname = 'Sheet1') {
    var tab_text = '<html xmlns: x="urn:schemas-microsoft-com:office:excel">';
    tab_text = tab_text + '<head><xml><x: ExcelWorkbook><x: ExcelWorksheets><x: ExcelWorksheet>';
    tab_text = tab_text + '<x: Name>' + sheetname + '</x: Name>';
    tab_text = tab_text + '<x: WorksheetOptions><x: Panes></x: Panes></x: WorksheetOptions ></x: ExcelWorksheet > ';
    tab_text = tab_text + '</x:ExcelWorksheets></x:ExcelWorkbook></xml></head><body>';
    tab_text = tab_text + "<table border='1px'>";

    //get table HTML code
    tab_text = tab_text + $('#' + tableID).html();
    tab_text = tab_text + '</table></body></html>';

    var data_type = 'data:application/vnd.ms-excel';

    var ua = window.navigator.userAgent;
    var msie = ua.indexOf("MSIE ");
    //For IE
    if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
        if (window.navigator.msSaveBlob) {
            var blob = new Blob([tab_text], { type: "application/csv;charset=utf-8;" });
            navigator.msSaveBlob(blob, filename + '.xls');
        }
    }
    //for Chrome and Firefox 
    else {
        var anchorElement = $('<a id="file"></a>');
        anchorElement.attr('href', data_type + ', ' + encodeURIComponent(tab_text));
        anchorElement.attr('download', filename + '.xls');
        $('body').append(anchorElement);
        document.getElementById('file').click();
    }
}
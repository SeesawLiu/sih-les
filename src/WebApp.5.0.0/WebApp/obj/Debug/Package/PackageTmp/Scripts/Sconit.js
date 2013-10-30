
window.onload = function () {
    //$("#content").height(getTotalHeight() - 38);
    $(".t-grid-content").removeAttr("style");
    //$(".t-grid-content").attr("style", "overflow-y:no");
}

function getTotalHeight() {
    if ($.browser.msie) {
        return document.compatMode == "CSS1Compat" ? document.documentElement.clientHeight : document.body.clientHeight;
    }
    else {
        return self.innerHeight;
    }
}

function More() {
    $("#more").empty();
    if ($("#divMore").is(":hidden ")) {
        $("#divMore").fadeIn("slow");
        $("#more").append("Hide");
    }
    else {
        $("#divMore").hide("slow");
        $("#more").append("More...");
    }
}

function EmptyChange(e) {
    if (e.value != "") {
        var combobox = $(this).data("tComboBox");
        if (combobox != undefined && combobox.selectedIndex == undefined || combobox.selectedIndex == -1) {
            combobox.value("");
            combobox.reload();
        }
    }
}

function EmptyDataBinding() {
}

function PrintOrder(printUrl) {
    if (printUrl == null || printUrl.length == 0) {
        return;
    }
    var xlApp = null;
    try {
        xlApp = new ActiveXObject("Excel.Application");
    } catch (e) {
        //alert("${Common.Warning.Please.Send.The.Site.To.Join.Trust.Site}");
        alert("Please add a site to trust the current site!");
        return;
    }
    var xlBook = xlApp.WorkBooks.open(printUrl);
    var xlsheet = xlBook.Worksheets(1);
    try {
        xlsheet.PrintOut(); //打印工作表
    } catch (e) {
    }
    xlBook.Close(false); //关闭文档
    xlApp.Quit();   //结束excel对象
    xlApp = null;   //释放excel对象
}

function CleanTabMessage() {
    $("#successesul").html('');
    $("#errorsul").html('');
    $("#warningsul").html('');
}

function TelerikonUpload_OnSuccess(e) {
    $("#errorsul").html(e.XMLHttpRequest.responseText);
    $('.t-upload-files').remove();
}

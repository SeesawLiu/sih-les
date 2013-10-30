
window.onload = function () {
    //$("#content").height(getTotalHeight() - 38);
    $(".t-grid-content").removeAttr("style");

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
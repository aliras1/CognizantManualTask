// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function sendResult(id, result) {
    let params = {
        "result": result
    };

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "./Home/SendResult/" + id,
        dataType: "json",
        data: JSON.stringify(params),
        success: function (response) {
            if (response !== "OK") {
                alert(response);
                return;
            }          

            if (result === "PASS") {
                $('#' + id).remove();
            }            
        }
    });
}

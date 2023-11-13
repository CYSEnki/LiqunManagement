//start you write

//customer method
function clientAlert(alertmessage) {
    //https://codeseven.github.io/toastr/demo.html
    var jsbody = $('<p></p>').append(alertmessage).html();
    console.log(jsbody);
    if (jsbody.length > 0) {
        var msg = JSON.parse(jsbody);
        switch (msg.mode) {
            case "bootstrap":
                toastr.options = {
                    "closeButton": true,
                    "debug": false,
                    "newestOnTop": false,
                    "progressBar": true,
                    "positionClass": msg.position,
                    "preventDuplicates": false,
                    "onclick": null,
                    "showDuration": "300",
                    "hideDuration": "1000",
                    "timeOut": msg.delay,
                    "extendedTimeOut": "1000",
                    "showEasing": "swing",
                    "hideEasing": "linear",
                    "showMethod": "fadeIn",
                    "hideMethod": "fadeOut"
                }
                toastr[msg.type](msg.message, msg.title);
                //$(document).Toasts('create', {
                //    class: msg.class,
                //    title: msg.title,
                //    subtitle: msg.subtitle,
                //    autohide: msg.autohide,
                //    "progressBar": true,
                //    "positionClass": "toast-top-full-width",
                //    delay: msg.delay,
                //    body: msg.message
                //})
                break;
            default:
                alert(msg.message);
                break;
        }
    }
}

function toggleLoading(show) {
    $('#loading').modal({
        backdrop: 'static',
        keyboard: false
    }).modal(show ? 'show' : 'hide');
}
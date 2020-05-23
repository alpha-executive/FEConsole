$(document).ready(function () {
    $('.icheck input').iCheck({
        checkboxClass: 'icheckbox_polaris',
        radioClass: 'iradio_polaris',
        increaseArea: '20%' // optional
    });
});
$(document).ajaxStart(function () { Pace.restart(); });
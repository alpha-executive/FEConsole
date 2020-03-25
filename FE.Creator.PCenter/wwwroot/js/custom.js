(function($) {

  // Init Wow
  wow = new WOW({
    animateClass: 'animated',
    offset: 100
  });
  wow.init();

  $(".navbar-collapse a").on('click', function() {
    $(".navbar-collapse.collapse").removeClass('in');
  });

  // Navigation scrolls
  $('.navbar-nav li a').bind('click', function(event) {
    $('.navbar-nav li').removeClass('active');
    $(this).closest('li').addClass('active');
    var $anchor = $(this);
    var nav = $($anchor.attr('href'));
    if (nav.length) {
      $('html, body').stop().animate({
        scrollTop: $($anchor.attr('href')).offset().top
      }, 1500, 'easeInOutExpo');

      event.preventDefault();
    }
  });

  // About section scroll
  $(".overlay-detail a").on('click', function(event) {
    event.preventDefault();
    var hash = this.hash;
    $('html, body').animate({
      scrollTop: $(hash).offset().top
    }, 900, function() {
      window.location.hash = hash;
    });
  });

  //jQuery to collapse the navbar on scroll
  $(window).scroll(function() {
    if ($(".navbar-default").offset().top > 50) {
      $(".navbar-fixed-top").addClass("top-nav-collapse");
    } else {
      $(".navbar-fixed-top").removeClass("top-nav-collapse");
    }
  });


  $('.btndonate').on('click', function(event) {
    event.preventDefault();
    $('.mainwindow').toggleClass("invisible-item");
    $('.paybarcode').toggleClass("invisible-item");

    if ($(this).hasClass("alipay")){
      $(".paybarcode .alipay").show();
      $(".paybarcode .wechatpay").hide();
    }

    if ($(this).hasClass("wechatpay"))
    {
      $(".paybarcode .wechatpay").show();
      $(".paybarcode .alipay").hide();
    }
  });

 getCookie = function(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for(var i = 0; i <ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

 $('#btnsendmessage').on('click', function(event){
    event.preventDefault();
    var xcrf_token = getCookie("XSRF-TOKEN");
    var subject = $("#subject").val();
    var message = $("#message").val();
    $(".loader").show();
    $.ajax({
      url: "/home/sendmessage",
      method:"POST",
      headers: { "X-XSRF-TOKEN" : xcrf_token },
      data: {subject: subject, message: message}
    })
    .done(function(){
      $("#successalert").toggleClass("show");
    })
    .fail(function() {
      $("#failedalert").toggleClass("show");
    })
    .always(function(){
      $(".loader").hide();
    });
  });

  $(".btnclosealert").on("click", function(event){
    $(this).parent().toggleClass("show");
  });

})(jQuery);

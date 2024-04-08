(function ($) {
    'use strict';

    // Your existing code for submenu hover

    // Toggle class on button click
    $(document).on('click', '#minimize', function () {
        $('body').toggleClass('sidebar-icon-only');
    });

    $(document).on('click', '#sidebar-offcanvas', function () {
        $('.sidebar-offcanvas').toggleClass('active');
    });

})(jQuery);
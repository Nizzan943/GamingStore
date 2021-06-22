$(document).ready(function () {
	initQuantity();
	initImage();

	function initQuantity() {
		// Handle product quantity input
		if ($('.product_quantity').length) {
			var input = $('#quantity_input');
			var incButton = $('#quantity_inc_button');
			var decButton = $('#quantity_dec_button');

			var originalVal;
			var endVal;

			incButton.on('click', function () {
				originalVal = input.val();
				endVal = parseFloat(originalVal) + 1;
				input.val(endVal);
			});

			decButton.on('click', function () {
				originalVal = input.val();
				if (originalVal > 0) {
					endVal = parseFloat(originalVal) - 1;
					input.val(endVal);
				}
			});
		}
	}


	function initImage() {
		var images = $('.image_list li');
		var selected = $('.image_selected img');

		images.each(function () {
			var image = $(this);
			image.on('click', function () {
				var imagePath = new String(image.data('image'));
				selected.attr('src', imagePath);
			});
		});
	}
});
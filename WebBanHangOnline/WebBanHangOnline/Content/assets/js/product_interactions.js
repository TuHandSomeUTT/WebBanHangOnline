$(document).ready(function () {
    let isProcessing = false; // Cờ để chống double click

    // Off và On lại sự kiện click trên chính phần tử .btnAddToCart.
    // Điều này đảm bảo handler của bạn là cái đầu tiên được gắn trực tiếp.
    $('.btnAddToCart').off('click').on('click', function (e) {
        e.preventDefault();
        // ✅ GIỮ LẠI DÒNG NÀY: Ngăn chặn các handler khác (bao gồm cả delegated events)
        // và sự kiện nổi bọt lên các phần tử cha.
        e.stopImmediatePropagation();

        if (isProcessing) {
            return; // Nếu đang xử lý, bỏ qua các click tiếp theo
        }
        isProcessing = true; // Bật cờ, bắt đầu xử lý

        var button = $(this);
        var productId = button.data('id');
        var availableQty = parseInt(button.data('quantity'));

        // Bước 1: Kiểm tra hiện tại trong cart đã có bao nhiêu cái
        $.ajax({
            url: '/shoppingcart/checkquantity',
            type: 'POST',
            data: { id: productId },
            success: function (res) {
                var cartQty = parseInt(res.Quantity || 0);

                // Bước 2: Nếu vượt quá tồn kho thì cảnh báo
                if (cartQty + 1 > availableQty) {
                    alert("Bạn đã thêm tối đa số lượng hiện có trong kho (" + availableQty + ").");
                    isProcessing = false; // reset cờ khi hoàn thành
                    return;
                }

                // Bước 3: Nếu OK thì AddToCart
                $.ajax({
                    url: '/shoppingcart/addtocart',
                    type: 'POST',
                    data: { id: productId, quantity: 1 },
                    success: function (res2) {
                        if (res2.Success) {
                            alert(res2.msg);
                            // ✅ CHỈNH SỬA DÒNG NÀY: Thay thế '#cart-count' bằng '#checkout_items'
                            $('#checkout_items').text(res2.Count);

                        } else {
                            alert(res2.msg || "Không thể thêm sản phẩm.");
                        }
                        isProcessing = false; // reset cờ khi hoàn thành
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.error("Lỗi AJAX AddToCart:", textStatus, errorThrown, jqXHR.responseText);
                        alert("Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng.");
                        isProcessing = false; // reset cờ nếu lỗi
                    }
                });
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("Lỗi AJAX CheckQuantity:", textStatus, errorThrown, jqXHR.responseText);
                alert("Đã xảy ra lỗi khi kiểm tra số lượng sản phẩm.");
                isProcessing = false; // reset cờ nếu lỗi
            }
        });
    });
});
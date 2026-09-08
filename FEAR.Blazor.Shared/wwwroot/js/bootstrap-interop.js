function initializeModal(modalId, backdropType, top, left) {
    let $modal = $(modalId);
    if(top !== undefined && left != undefined)
        $modal.css({
            left: left,
            top: top
        })

    let backdropDisplay = backdropType;
    if (backdropType == "true")
        backdropDisplay = true;
    if (backdropType == "false")
        backdropDisplay = false;

    let bsModal = new bootstrap.Modal($modal, {
        backdrop: backdropDisplay,
        keyboard: false
    });

    bsModal.show();
};

function draggableModal(modalId) {
    let $modal = $(modalId);
    $modal.draggable({
        handle: ".modal-header",
    });
}

function closeModal(modalId) {
    let $modal = $(modalId);
    let bsModal = bootstrap.Modal.getInstance($modal[0]);
    if (bsModal) {
        bsModal.hide();
    }
};
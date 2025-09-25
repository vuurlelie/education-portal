(() => {
    const MODAL_ID = "app-modal";
    const MODAL_BODY_ID = "app-modal-body";

    function getModalElements() {
        const modalEl = document.getElementById(MODAL_ID);
        if (!modalEl) throw new Error(`Modal element #${MODAL_ID} not found.`);
        const bodyEl = document.getElementById(MODAL_BODY_ID);
        if (!bodyEl) throw new Error(`Modal body element #${MODAL_BODY_ID} not found.`);
        return { modalEl, bodyEl };
    }
    function initMaterialForm(root) {
        const container = root || document;
        const typeSelect = container.querySelector("#material-type");
        if (!typeSelect) return;

        const sections = container.querySelectorAll(".type-section");

        const update = () => {
            const type = typeSelect.value;
            sections.forEach(sec => {
                const key = sec.getAttribute("data-section");
                const show =
                    (type === "Video" && key === "video") ||
                    (type === "Book" && key === "book") ||
                    (type === "Article" && key === "article");
                sec.classList.toggle("d-none", !show);
            });
        };

        typeSelect.removeEventListener("change", update);
        typeSelect.addEventListener("change", update);
        update();
    }

    async function openAppModal(requestUrl, modalTitle) {
        const { modalEl, bodyEl } = getModalElements();

        const titleEl = modalEl.querySelector(".modal-title");
        if (titleEl) titleEl.textContent = modalTitle || "";

        bodyEl.innerHTML = "";

        let response;
        try {
            response = await fetch(requestUrl, { headers: { "X-Requested-With": "XMLHttpRequest" } });
        } catch (err) {
            bodyEl.innerHTML = `<div class="alert alert-danger">Network error. Please try again.</div>`;
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
            return;
        }

        if (response.redirected) {
            window.location = response.url;
            return;
        }
        if (response.status === 401 || response.status === 403 ||
            (response.url && response.url.includes("/Identity/Account/Login"))) {
            window.location = response.url || "/Identity/Account/Login";
            return;
        }

        const html = await response.text();
        bodyEl.innerHTML = html;

        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
            window.jQuery.validator.unobtrusive.parse(bodyEl);
        }

        initMaterialForm(bodyEl);

        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }

    document.addEventListener("click", (e) => {
        const trigger = e.target.closest("[data-modal-url]");
        if (!trigger) return;

        e.preventDefault();
        const url = trigger.getAttribute("data-modal-url");
        const title = trigger.getAttribute("data-modal-title") || trigger.textContent.trim();
        if (!url) return;

        openAppModal(url, title);
    });

    window.initMaterialForm = initMaterialForm;
    window.openAppModal = openAppModal;
})();
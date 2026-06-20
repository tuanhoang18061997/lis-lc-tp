(function () {
    const ZaloOATemplate = {
        init: function () {
            this.bindEvents();
        },

        bindEvents: function () {
            this.bindClick("btnNewTop", this.createForm.bind(this));
            this.bindClick("btnSaveTop", this.save.bind(this));
            this.bindClick("btnNewBottom", this.createForm.bind(this));
            this.bindClick("btnSaveBottom", this.save.bind(this));
        },

        bindClick: function (id, handler) {
            const el = document.getElementById(id);
            if (el) el.addEventListener("click", handler);
        },

        createForm: function () {
            this.setValue("Id", "");
            this.setValue("Code", "");
            this.setValue("Name", "");
            this.setValue("ZaloOAConfigId", "");
            this.setValue("HospitalId", "");
            this.setValue("TemplateZaloId", "");
            this.setValue("Title", "");
            this.setValue("Content", "");
            this.setValue("Notes", "");
            this.setChecked("IsDefault", false);
            this.setChecked("IsActive", true);
            this.focus("Code");
        },

        getModel: function () {
            const id = this.getValue("Id");
            const hospitalId = this.getValue("HospitalId");
            const zaloOAConfigId = this.getValue("ZaloOAConfigId");

            return {
                id: id ? parseInt(id) : 0,
                code: this.getValue("Code"),
                name: this.getValue("Name"),
                zaloOAConfigId: zaloOAConfigId ? parseInt(zaloOAConfigId) : 0,
                hospitalId: hospitalId ? parseInt(hospitalId) : null,
                templateZaloId: this.getValue("TemplateZaloId"),
                title: this.getValue("Title"),
                content: this.getValue("Content"),
                notes: this.getValue("Notes"),
                isDefault: this.getChecked("IsDefault"),
                isActive: this.getChecked("IsActive")
            };
        },

        validate: function (model) {
            if (!model.code || model.code.trim() === "") {
                Swal.fire({
                    icon: 'warning',
                    title: 'Thiếu thông tin',
                    text: 'Vui lòng nhập Mã'
                });
                this.focus("Code");
                return false;
            }

            if (!model.name || model.name.trim() === "") {
                Swal.fire({
                    icon: 'warning',
                    title: 'Thiếu thông tin',
                    text: 'Vui lòng nhập Tên'
                });
                this.focus("Name");
                return false;
            }

            if (!model.zaloOAConfigId || model.zaloOAConfigId <= 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Thiếu thông tin',
                    text: 'Vui lòng chọn ZaloOA Config'
                });
                this.focus("ZaloOAConfigId");
                return false;
            }

            if (!model.templateZaloId || model.templateZaloId.trim() === "") {
                Swal.fire({
                    icon: 'warning',
                    title: 'Thiếu thông tin',
                    text: 'Vui lòng nhập TemplateZaloId'
                });
                this.focus("TemplateZaloId");
                return false;
            }

            if (!model.title || model.title.trim() === "") {
                Swal.fire({
                    icon: 'warning',
                    title: 'Thiếu thông tin',
                    text: 'Vui lòng nhập Title'
                });
                this.focus("Title");
                return false;
            }

            if (model.isDefault && model.hospitalId !== null) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Lỗi dữ liệu',
                    text: 'Template mặc định không được gán HospitalId'
                });
                this.focus("HospitalId");
                return false;
            }

            return true;
        },

        save: async function () {
            const model = this.getModel();

            if (!this.validate(model)) return;

            const url = model.id > 0
                ? "/ZaloOATemplate/Update"
                : "/ZaloOATemplate/Create";

            try {
                const response = await fetch(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(model)
                });

                const result = await response.json();

                if (result.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: result.message || 'Lưu thành công'
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: result.message || 'Lưu thất bại'
                    });
                }
            } catch (e) {
                console.error(e);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Có lỗi xảy ra khi lưu'
                });
            }
        },

        edit: async function (id) {
            try {
                const response = await fetch("/ZaloOATemplate/GetById?id=" + id);
                const data = await response.json();

                this.setValue("Id", data.id || "");
                this.setValue("Code", data.code || "");
                this.setValue("Name", data.name || "");
                this.setValue("ZaloOAConfigId", data.zaloOAConfigId || "");
                this.setValue("HospitalId", data.hospitalId ?? "");
                this.setValue("TemplateZaloId", data.templateZaloId || "");
                this.setValue("Title", data.title || "");
                this.setValue("Content", data.content || "");
                this.setValue("Notes", data.notes || "");
                this.setChecked("IsDefault", data.isDefault === true);
                this.setChecked("IsActive", data.isActive === true);

                this.focus("Code");
            } catch (e) {
                console.error(e);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Có lỗi khi lấy dữ liệu'
                });
            }
        },

        delete: async function (id) {
            const result = await Swal.fire({
                icon: 'warning',
                title: 'Xác nhận xóa',
                text: 'Bạn có chắc muốn xóa template này không?',
                showCancelButton: true,
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy',
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6'
            });

            if (!result.isConfirmed) return;

            try {
                const response = await fetch("/ZaloOATemplate/Delete?id=" + id, {
                    method: "POST"
                });

                const deleteResult = await response.json();

                if (deleteResult.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: deleteResult.message || 'Xóa thành công'
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: deleteResult.message || 'Xóa thất bại'
                    });
                }
            } catch (e) {
                console.error(e);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Có lỗi khi xóa'
                });
            }
        },

        setValue: function (id, value) {
            const el = document.getElementById(id);
            if (el) el.value = value;
        },

        getValue: function (id) {
            const el = document.getElementById(id);
            return el ? el.value : "";
        },

        setChecked: function (id, checked) {
            const el = document.getElementById(id);
            if (el) el.checked = checked;
        },

        getChecked: function (id) {
            const el = document.getElementById(id);
            return el ? el.checked : false;
        },

        focus: function (id) {
            const el = document.getElementById(id);
            if (el) el.focus();
        }
    };

    window.ZaloOATemplate = ZaloOATemplate;
    window.editTemplate = function (id) { ZaloOATemplate.edit(id); };
    window.deleteTemplate = function (id) { ZaloOATemplate.delete(id); };

    document.addEventListener("DOMContentLoaded", function () {
        ZaloOATemplate.init();
    });
})();
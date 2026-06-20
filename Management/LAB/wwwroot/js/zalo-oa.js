(function () {
    const ZaloOA = {
        init: function () {
            this.bindEvents();
        },

        bindEvents: function () {
            const btnNewTop = document.getElementById("btnNewTop");
            const btnSaveTop = document.getElementById("btnSaveTop");
            const btnNewBottom = document.getElementById("btnNewBottom");
            const btnSaveBottom = document.getElementById("btnSaveBottom");

            if (btnNewTop) {
                btnNewTop.addEventListener("click", this.createConfig.bind(this));
            }

            if (btnSaveTop) {
                btnSaveTop.addEventListener("click", this.saveConfig.bind(this));
            }

            if (btnNewBottom) {
                btnNewBottom.addEventListener("click", this.createConfig.bind(this));
            }

            if (btnSaveBottom) {
                btnSaveBottom.addEventListener("click", this.saveConfig.bind(this));
            }
        },

        createConfig: function () {
            this.setValue("Id", "");
            this.setValue("Code", "");
            this.setValue("Name", "");
            this.setValue("HospitalId", "");
            this.setValue("ApiUrl", "");
            this.setValue("Username", "");
            this.setValue("Password", "");
            this.setValue("ZaloOaId", "");
            this.setValue("Notes", "");
            this.setChecked("IsDefault", false);
            this.setChecked("IsActive", true);

            this.focus("Code");
        },

        getModel: function () {
            const idValue = this.getValue("Id");
            const hospitalIdValue = this.getValue("HospitalId");

            return {
                id: idValue ? parseInt(idValue) : 0,
                code: this.getValue("Code"),
                name: this.getValue("Name"),
                hospitalId: hospitalIdValue ? parseInt(hospitalIdValue) : null,
                apiUrl: this.getValue("ApiUrl"),
                username: this.getValue("Username"),
                password: this.getValue("Password"),
                zaloOaId: this.getValue("ZaloOaId"),
                isDefault: this.getChecked("IsDefault"),
                isActive: this.getChecked("IsActive"),
                notes: this.getValue("Notes")
            };
        },

        validateModel: function (model) {
            if (!model.code || model.code.trim() === "") {
                SwalHelper.Toast.warning("Vui lòng nhập Mã");
                this.focus("Code");
                return false;
            }

            if (!model.name || model.name.trim() === "") {
                SwalHelper.Toast.warning("Vui lòng nhập Tên");
                this.focus("Name");
                return false;
            }

            if (!model.apiUrl || model.apiUrl.trim() === "") {
                SwalHelper.Toast.warning("Vui lòng nhập ApiUrl");
                this.focus("ApiUrl");
                return false;
            }

            if (!model.username || model.username.trim() === "") {
                SwalHelper.Toast.warning("Vui lòng nhập Username");
                this.focus("Username");
                return false;
            }

            if (!model.password || model.password.trim() === "") {
                SwalHelper.Toast.warning("Vui lòng nhập Password");
                this.focus("Password");
                return false;
            }

            if (model.isDefault && model.hospitalId !== null) {
                SwalHelper.Toast.warning("Cấu hình mặc định không được gán HospitalId");
                this.focus("HospitalId");
                return false;
            }

            return true;
        },

        saveConfig: async function () {
            const model = this.getModel();

            if (!this.validateModel(model)) {
                return;
            }

            const url = model.id > 0
                ? "/ZaloOAConfig/Update"
                : "/ZaloOAConfig/Create";

            try {
                this.setLoading(true);

                const response = await fetch(url, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(model)
                });

                const result = await response.json();

                if (result.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: model.id > 0 ? 'Cập nhật thành công' : 'Thêm mới thành công'
                    }).then(() => {
                        window.location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: result.message || 'Lưu thất bại'
                    });
                }
            } catch (error) {
                console.error("saveConfig error:", error);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Có lỗi xảy ra khi lưu dữ liệu'
                });
            } finally {
                this.setLoading(false);
            }
        },

        editConfig: async function (id) {
            if (!id) return;

            try {
                this.setLoading(true);

                const response = await fetch("/ZaloOAConfig/GetById?id=" + id, {
                    method: "GET"
                });

                if (!response.ok) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Không lấy được dữ liệu cấu hình'
                    });
                    return;
                }

                const data = await response.json();

                this.setValue("Id", data.id || "");
                this.setValue("Code", data.code || "");
                this.setValue("Name", data.name || "");
                this.setValue("HospitalId", data.hospitalId ?? "");
                this.setValue("ApiUrl", data.apiUrl || "");
                this.setValue("Username", data.username || "");
                this.setValue("Password", data.password || "");
                this.setValue("ZaloOaId", data.zaloOaId || "");
                this.setValue("Notes", data.notes || "");
                this.setChecked("IsDefault", data.isDefault === true);
                this.setChecked("IsActive", data.isActive === true);

                this.focus("Code");
            } catch (error) {
                console.error("editConfig error:", error);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Có lỗi xảy ra khi lấy dữ liệu'
                });
            } finally {
                this.setLoading(false);
            }
        },

        deleteConfig: async function (id) {
            if (!id) return;

            const confirmed = await Swal.fire({
                icon: 'warning',
                title: 'Xác nhận xóa',
                text: 'Bạn có chắc muốn xóa cấu hình này không?',
                showCancelButton: true,
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy',
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6'
            });

            if (!confirmed.isConfirmed) return;

            try {
                this.setLoading(true);

                const response = await fetch("/ZaloOAConfig/Delete?id=" + id, {
                    method: "POST"
                });

                const result = await response.json();

                if (result.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: 'Xóa thành công'
                    }).then(() => {
                        window.location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: result.message || 'Xóa thất bại'
                    });
                }
            } catch (error) {
                console.error("deleteConfig error:", error);
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Có lỗi xảy ra khi xóa dữ liệu'
                });
            } finally {
                this.setLoading(false);
            }
        },

        setValue: function (id, value) {
            const el = document.getElementById(id);
            if (el) {
                el.value = value;
            }
        },

        getValue: function (id) {
            const el = document.getElementById(id);
            return el ? el.value : "";
        },

        setChecked: function (id, checked) {
            const el = document.getElementById(id);
            if (el) {
                el.checked = checked;
            }
        },

        getChecked: function (id) {
            const el = document.getElementById(id);
            return el ? el.checked : false;
        },

        focus: function (id) {
            const el = document.getElementById(id);
            if (el) {
                el.focus();
            }
        },

        setLoading: function (isLoading) {
            const buttons = document.querySelectorAll(
                "#btnNewTop, #btnSaveTop, #btnNewBottom, #btnSaveBottom"
            );

            buttons.forEach(function (btn) {
                btn.disabled = isLoading;
            });
        }
    };

    window.ZaloOA = ZaloOA;

    window.createConfig = function () {
        ZaloOA.createConfig();
    };

    window.saveConfig = function () {
        ZaloOA.saveConfig();
    };

    window.editConfig = function (id) {
        ZaloOA.editConfig(id);
    };

    window.deleteConfig = function (id) {
        ZaloOA.deleteConfig(id);
    };

    window.viewAuthorizationKey = function () {
        const username = document.getElementById("Username")?.value || "";
        const password = document.getElementById("Password")?.value || "";

        if (!username || !password) {
            Swal.fire({
                icon: 'warning',
                title: 'Thiếu thông tin',
                text: 'Vui lòng nhập Username và Password trước'
            });
            return;
        }

        const raw = username + ":" + password;

        try {
            // encode base64 (unicode safe)
            const encoded = btoa(unescape(encodeURIComponent(raw)));

            document.getElementById("AuthorizationKey").value = encoded;
        } catch (e) {
            console.error("Base64 encode error:", e);
            SwalHelper.Toast.error("Không thể tạo Authorization Key");
        }
    };

    document.addEventListener("DOMContentLoaded", function () {
        ZaloOA.init();
    });
})();
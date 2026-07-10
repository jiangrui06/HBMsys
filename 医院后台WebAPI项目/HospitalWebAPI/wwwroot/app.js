const API_BASE = '/api';

// Navigation
const navLinks = document.querySelectorAll('.nav-link[data-section]');
navLinks.forEach(link => {
    link.addEventListener('click', e => {
        e.preventDefault();
        const section = link.getAttribute('data-section');
        showSection(section);
        navLinks.forEach(l => l.classList.remove('active'));
        link.classList.add('active');
    });
});

function showSection(sectionId) {
    document.querySelectorAll('.page-section').forEach(s => s.classList.add('d-none'));
    document.getElementById(sectionId).classList.remove('d-none');
}

// Modal helpers
const modalEl = document.getElementById('messageModal');
const modal = new bootstrap.Modal(modalEl);

function showModal(title, message, type = 'info') {
    document.getElementById('modalTitle').textContent = title;
    document.getElementById('modalBody').innerHTML = message;
    document.getElementById('modalConfirmBtn').style.display = 'none';
    modal.show();
}

function showConfirmModal(title, message, onConfirm) {
    document.getElementById('modalTitle').textContent = title;
    document.getElementById('modalBody').innerHTML = message;
    const btn = document.getElementById('modalConfirmBtn');
    btn.style.display = 'inline-block';
    btn.onclick = () => {
        modal.hide();
        onConfirm();
    };
    modal.show();
}

// Validation helpers
function isEmpty(value) {
    return value === null || value === undefined || value.toString().trim() === '';
}

function isNegativeNumber(value) {
    const n = Number(value);
    return !isNaN(n) && n < 0;
}

function markInvalid(el) {
    el.classList.add('is-invalid');
    setTimeout(() => el.classList.remove('is-invalid'), 3000);
}

function validateChineseOrEnglishName(name) {
    return /^[\u4e00-\u9fa5a-zA-Z]+$/.test(name);
}

function validatePhone(phone) {
    return /^1[3-9]\d{9}$/.test(phone);
}

// API helpers
async function get(url) {
    const res = await fetch(API_BASE + url);
    return res.json();
}

async function post(url, body) {
    const res = await fetch(API_BASE + url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    return res.json();
}

async function put(url) {
    const res = await fetch(API_BASE + url, { method: 'PUT' });
    return res.json();
}

async function del(url) {
    const res = await fetch(API_BASE + url, { method: 'DELETE' });
    return res.json();
}

// Delayed refresh helper
function delayedRefresh(fn, ms = 500) {
    setTimeout(fn, ms);
}

// Render empty table message
function renderEmptyTable(message = '暂无数据') {
    return `<div class="table-empty">
        <div class="table-empty-icon">📋</div>
        <div>${message}</div>
    </div>`;
}

// Render generic table
function renderTable(headers, rows, emptyMessage = '暂无数据') {
    if (!rows || rows.length === 0) return renderEmptyTable(emptyMessage);
    let html = '<div class="table-responsive"><table class="table table-striped table-hover"><thead><tr>';
    headers.forEach(h => html += `<th>${h.label}</th>`);
    html += '</tr></thead><tbody>';
    rows.forEach(row => {
        html += '<tr>';
        headers.forEach(h => {
            const val = h.key.split('.').reduce((o, k) => o && o[k], row);
            html += `<td>${val !== undefined && val !== null ? val : ''}</td>`;
        });
        html += '</tr>';
    });
    html += '</tbody></table></div>';
    return html;
}

// Home
async function loadHomeIndex() {
    const data = await get('/Patient/homeindex');
    document.getElementById('homeResult').innerHTML = `<div class="alert alert-info">${data.data}</div>`;
}

// Patient Registration
async function addReg() {
    const nameEl = document.getElementById('regName');
    const phoneEl = document.getElementById('regPhone');
    const deptEl = document.getElementById('regDept');
    const name = nameEl.value.trim();
    const phone = phoneEl.value.trim();
    const dept = deptEl.value.trim();

    if (isEmpty(name)) { markInvalid(nameEl); return showModal('提示', '患者姓名不能为空'); }
    if (!validateChineseOrEnglishName(name)) { markInvalid(nameEl); return showModal('提示', '患者姓名只能包含中文或英文字母'); }
    if (isEmpty(phone)) { markInvalid(phoneEl); return showModal('提示', '手机号不能为空'); }
    if (!validatePhone(phone)) { markInvalid(phoneEl); return showModal('提示', '手机号格式不正确'); }
    if (isEmpty(dept)) { markInvalid(deptEl); return showModal('提示', '科室不能为空'); }

    const result = await post('/Patient/addreg', { patientName: name, phone, department: dept });
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) {
        nameEl.value = ''; phoneEl.value = ''; deptEl.value = '';
        delayedRefresh(loadAllReg);
    }
}

async function loadMyReg() {
    const name = document.getElementById('myRegName').value.trim();
    if (isEmpty(name)) return showModal('提示', '请输入姓名');
    const result = await get(`/Patient/myreg?name=${encodeURIComponent(name)}`);
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '患者姓名', key: 'patientName' },
        { label: '手机号', key: 'phone' },
        { label: '科室', key: 'department' },
        { label: '挂号时间', key: 'createdAt' }
    ];
    document.getElementById('regTable').innerHTML = renderTable(headers, result.data, result.message);
}

async function loadAllReg() {
    const result = await get('/Patient/allreg');
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '患者姓名', key: 'patientName' },
        { label: '手机号', key: 'phone' },
        { label: '科室', key: 'department' },
        { label: '挂号时间', key: 'createdAt' }
    ];
    document.getElementById('regTable').innerHTML = renderTable(headers, result.data, '暂无挂号记录');
}

// Pharmacy
let medicineListCache = [];

async function loadMedicines() {
    const result = await get('/Pharmacy/getallmedicine');
    medicineListCache = result.data || [];
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '药品名称', key: 'name' },
        { label: '规格', key: 'specification' },
        { label: '单价', key: 'price' },
        { label: '库存', key: 'stock' },
        { label: '操作', key: 'id' }
    ];
    if (!result.data || result.data.length === 0) {
        document.getElementById('medicineTable').innerHTML = renderEmptyTable('暂无药品数据');
    } else {
        let html = '<div class="table-responsive"><table class="table table-striped table-hover"><thead><tr>';
        headers.forEach(h => html += `<th>${h.label}</th>`);
        html += '</tr></thead><tbody>';
        result.data.forEach(m => {
            html += `<tr>
                <td>${m.id}</td><td>${m.name}</td><td>${m.specification}</td><td>${m.price}</td><td>${m.stock}</td>
                <td><button class="btn btn-sm btn-danger" onclick="confirmDeleteMedicine(${m.id})">删除</button></td>
            </tr>`;
        });
        html += '</tbody></table></div>';
        document.getElementById('medicineTable').innerHTML = html;
    }
    updateMedicineSelects();
}

function updateMedicineSelects() {
    document.querySelectorAll('.med-select').forEach(select => {
        const current = select.value;
        let html = '<option value="">选择药品</option>';
        medicineListCache.forEach(m => {
            html += `<option value="${m.id}">${m.name} (库存: ${m.stock})</option>`;
        });
        select.innerHTML = html;
        select.value = current;
    });
}

async function addMedicine() {
    const nameEl = document.getElementById('medName');
    const specEl = document.getElementById('medSpec');
    const priceEl = document.getElementById('medPrice');
    const stockEl = document.getElementById('medStock');

    const name = nameEl.value.trim();
    const spec = specEl.value.trim();
    const price = priceEl.value;
    const stock = stockEl.value;

    if (isEmpty(name)) { markInvalid(nameEl); return showModal('提示', '药品名称不能为空'); }
    if (isEmpty(spec)) { markInvalid(specEl); return showModal('提示', '规格不能为空'); }
    if (isEmpty(price) || isNegativeNumber(price)) { markInvalid(priceEl); return showModal('提示', '单价不能为负数或空'); }
    if (isEmpty(stock) || isNegativeNumber(stock)) { markInvalid(stockEl); return showModal('提示', '库存不能为负数或空'); }

    const result = await post('/Pharmacy/addmedicine', { name, specification: spec, price: Number(price), stock: Number(stock) });
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) {
        nameEl.value = ''; specEl.value = ''; priceEl.value = ''; stockEl.value = '';
        delayedRefresh(loadMedicines);
    }
}

function confirmDeleteMedicine(id) {
    showConfirmModal('确认删除', '确定要删除该药品吗？', async () => {
        const result = await del(`/Pharmacy/deletemedicine?id=${id}&confirmed=true`);
        showModal(result.success ? '成功' : '失败', result.message);
        if (result.success) delayedRefresh(loadMedicines);
    });
}

function addPresItem() {
    const div = document.createElement('div');
    div.className = 'row g-3 pres-item mb-2';
    div.innerHTML = `
        <div class="col-md-5"><select class="form-select med-select"></select></div>
        <div class="col-md-3"><input type="number" class="form-control med-qty" placeholder="数量" min="1"></div>
        <div class="col-md-2"><button class="btn btn-danger w-100" onclick="removePresItem(this)">删除</button></div>
    `;
    document.getElementById('presItems').appendChild(div);
    updateMedicineSelects();
}

function removePresItem(btn) {
    const items = document.querySelectorAll('.pres-item');
    if (items.length <= 1) return showModal('提示', '至少需要保留一项药品');
    btn.closest('.pres-item').remove();
}

async function addPrescription() {
    const patientEl = document.getElementById('presPatient');
    const doctorEl = document.getElementById('presDoctor');
    const patientName = patientEl.value.trim();
    const doctorName = doctorEl.value.trim();

    if (isEmpty(patientName)) { markInvalid(patientEl); return showModal('提示', '患者姓名不能为空'); }
    if (isEmpty(doctorName)) { markInvalid(doctorEl); return showModal('提示', '医生姓名不能为空'); }

    const items = [];
    const selects = document.querySelectorAll('.med-select');
    const qtys = document.querySelectorAll('.med-qty');
    for (let i = 0; i < selects.length; i++) {
        const medId = selects[i].value;
        const qty = qtys[i].value;
        if (isEmpty(medId)) { markInvalid(selects[i]); return showModal('提示', '请选择药品'); }
        if (isEmpty(qty) || Number(qty) <= 0) { markInvalid(qtys[i]); return showModal('提示', '药品数量必须大于0'); }
        items.push({ medicineId: Number(medId), quantity: Number(qty) });
    }

    const result = await post('/Pharmacy/addprescription', { patientName, doctorName, items });
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) {
        patientEl.value = ''; doctorEl.value = '';
        document.getElementById('presItems').innerHTML = `
            <div class="row g-3 pres-item mb-2">
                <div class="col-md-5"><select class="form-select med-select"></select></div>
                <div class="col-md-3"><input type="number" class="form-control med-qty" placeholder="数量" min="1"></div>
                <div class="col-md-2"><button class="btn btn-danger w-100" onclick="removePresItem(this)">删除</button></div>
            </div>`;
        updateMedicineSelects();
        document.getElementById('presIdQuery').value = result.data.id;
        delayedRefresh(queryPrescription);
    }
}

async function queryPrescription() {
    const id = document.getElementById('presIdQuery').value;
    if (isEmpty(id)) return showModal('提示', '请输入处方ID');
    const result = await get(`/Pharmacy/getprescription?id=${id}`);
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '患者', key: 'patientName' },
        { label: '医生', key: 'doctorName' },
        { label: '总金额', key: 'totalAmount' },
        { label: '状态', key: 'status' },
        { label: '创建时间', key: 'createdAt' }
    ];
    document.getElementById('prescriptionTable').innerHTML = renderTable(headers, result.data, '未找到该处方');
}

async function dispense() {
    const id = document.getElementById('dispenseId').value;
    if (isEmpty(id)) return showModal('提示', '请输入处方ID');
    const result = await put(`/Pharmacy/dispense/${id}`);
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) {
        delayedRefresh(loadMedicines);
        delayedRefresh(queryPrescription);
    }
}

// Charge
async function createCharge() {
    const id = document.getElementById('createChargePresId').value;
    if (isEmpty(id)) return showModal('提示', '请输入处方ID');
    const result = await post(`/Charge/create/${id}`);
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) delayedRefresh(loadCharges);
}

async function payCharge() {
    const id = document.getElementById('payChargeId').value;
    if (isEmpty(id)) return showModal('提示', '请输入收费单ID');
    const result = await put(`/Charge/pay/${id}`);
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) delayedRefresh(loadCharges);
}

function cancelCharge() {
    const id = document.getElementById('cancelChargeId').value;
    if (isEmpty(id)) return showModal('提示', '请输入收费单ID');
    showConfirmModal('确认取消', '确定要取消该收费单吗？', async () => {
        const result = await put(`/Charge/cancel/${id}?confirmed=true`);
        showModal(result.success ? '成功' : '失败', result.message);
        if (result.success) delayedRefresh(loadCharges);
    });
}

async function loadCharges() {
    const result = await get('/Charge/getall');
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '处方ID', key: 'prescriptionId' },
        { label: '患者', key: 'patientName' },
        { label: '金额', key: 'amount' },
        { label: '状态', key: 'status' },
        { label: '票据号', key: 'receiptNo' },
        { label: '创建时间', key: 'createdAt' }
    ];
    document.getElementById('chargeTable').innerHTML = renderTable(headers, result.data, '暂无收费单');
}

// Medical Record
async function createRecord() {
    const presEl = document.getElementById('recordPresId');
    const diagEl = document.getElementById('recordDiagnosis');
    const presId = presEl.value;
    const diagnosis = diagEl.value.trim();

    if (isEmpty(presId)) { markInvalid(presEl); return showModal('提示', '请输入处方ID'); }
    if (isEmpty(diagnosis)) { markInvalid(diagEl); return showModal('提示', '诊断内容不能为空'); }

    const result = await post(`/Record/createrecord/${presId}`, { diagnosis });
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) {
        presEl.value = ''; diagEl.value = '';
        delayedRefresh(loadAllRecords);
    }
}

async function getRecordByPres() {
    const id = document.getElementById('recordByPresId').value;
    if (isEmpty(id)) return showModal('提示', '请输入处方ID');
    const result = await get(`/Record/getrecord/bypres/${id}`);
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '处方ID', key: 'prescriptionId' },
        { label: '患者', key: 'patientName' },
        { label: '诊断', key: 'diagnosis' },
        { label: '就诊时间', key: 'visitTime' }
    ];
    document.getElementById('recordTable').innerHTML = result.data
        ? renderTable(headers, [result.data], '未找到该处方的就诊记录')
        : renderEmptyTable('未找到该处方的就诊记录');
}

async function getRecordByTime() {
    const start = document.getElementById('recordStart').value;
    const end = document.getElementById('recordEnd').value;
    const result = await get(`/Record/getrecord/bytime?startTime=${encodeURIComponent(start)}&endTime=${encodeURIComponent(end)}`);
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '处方ID', key: 'prescriptionId' },
        { label: '患者', key: 'patientName' },
        { label: '诊断', key: 'diagnosis' },
        { label: '就诊时间', key: 'visitTime' }
    ];
    document.getElementById('recordTable').innerHTML = renderTable(headers, result.data, result.message);
}

async function loadAllRecords() {
    const result = await get('/Record/getrecord/all');
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '处方ID', key: 'prescriptionId' },
        { label: '患者', key: 'patientName' },
        { label: '诊断', key: 'diagnosis' },
        { label: '就诊时间', key: 'visitTime' }
    ];
    document.getElementById('recordTable').innerHTML = renderTable(headers, result.data, '暂无就诊记录');
}

// Report
async function addReport() {
    const patientEl = document.getElementById('reportPatient');
    const itemEl = document.getElementById('reportItem');
    const contentEl = document.getElementById('reportContent');
    const patientName = patientEl.value.trim();
    const examItem = itemEl.value.trim();
    const content = contentEl.value.trim();

    if (isEmpty(patientName)) { markInvalid(patientEl); return showModal('提示', '患者姓名不能为空'); }
    if (isEmpty(examItem)) { markInvalid(itemEl); return showModal('提示', '检查项目不能为空'); }
    if (isEmpty(content)) { markInvalid(contentEl); return showModal('提示', '报告内容不能为空'); }

    const result = await post('/Record/addreport', { patientName, examItem, content });
    showModal(result.success ? '成功' : '失败', result.message);
    if (result.success) {
        patientEl.value = ''; itemEl.value = ''; contentEl.value = '';
        delayedRefresh(loadAllReports);
    }
}

async function getReportByName() {
    const name = document.getElementById('reportByName').value.trim();
    if (isEmpty(name)) return showModal('提示', '请输入患者姓名');
    const result = await get(`/Record/getreport/byname?patientName=${encodeURIComponent(name)}`);
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '患者', key: 'patientName' },
        { label: '检查项目', key: 'examItem' },
        { label: '报告内容', key: 'content' },
        { label: '报告时间', key: 'reportTime' }
    ];
    document.getElementById('reportTable').innerHTML = renderTable(headers, result.data, result.message);
}

async function getReportByTime() {
    const start = document.getElementById('reportStart').value;
    const end = document.getElementById('reportEnd').value;
    const result = await get(`/Record/getreport/bytime?startTime=${encodeURIComponent(start)}&endTime=${encodeURIComponent(end)}`);
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '患者', key: 'patientName' },
        { label: '检查项目', key: 'examItem' },
        { label: '报告内容', key: 'content' },
        { label: '报告时间', key: 'reportTime' }
    ];
    document.getElementById('reportTable').innerHTML = renderTable(headers, result.data, result.message);
}

async function loadAllReports() {
    const result = await get('/Record/getreport/all');
    const headers = [
        { label: 'ID', key: 'id' },
        { label: '患者', key: 'patientName' },
        { label: '检查项目', key: 'examItem' },
        { label: '报告内容', key: 'content' },
        { label: '报告时间', key: 'reportTime' }
    ];
    document.getElementById('reportTable').innerHTML = renderTable(headers, result.data, '暂无检查报告');
}

// Full flow demo
async function runFullFlow() {
    const container = document.getElementById('flowResult');
    container.innerHTML = '<div class="alert alert-info">正在执行完整流程，请稍候...</div>';
    const randomLetters = (len) => {
        let s = '';
        for (let i = 0; i < len; i++) s += String.fromCharCode(65 + Math.floor(Math.random() * 26));
        return s;
    };
    const flowName = '流程演示患者' + randomLetters(4);
    const flowPhone = '138' + Math.floor(Math.random() * 100000000).toString().padStart(8, '0');
    const logs = [];

    try {
        // 1. Register
        let r = await post('/Patient/addreg', { patientName: flowName, phone: flowPhone, department: '骨科' });
        logs.push(`挂号: ${r.success ? '成功' : '失败'} - ${r.message}`);

        // 2. Prescription
        r = await post('/Pharmacy/addprescription', {
            patientName: flowName,
            doctorName: '张医生',
            items: [{ medicineId: 1, quantity: 2 }]
        });
        logs.push(`开方: ${r.success ? '成功' : '失败'} - ${r.message}`);
        const presId = r.data?.id;

        // 3. Medical record
        if (presId) {
            r = await post(`/Record/createrecord/${presId}`, { diagnosis: '感冒发热，需服药治疗' });
            logs.push(`就诊记录: ${r.success ? '成功' : '失败'} - ${r.message}`);
        }

        // 4. Charge
        let chargeId = null;
        if (presId) {
            r = await post(`/Charge/create/${presId}`);
            logs.push(`生成收费单: ${r.success ? '成功' : '失败'} - ${r.message}`);
            chargeId = r.data?.id;
        }

        // 5. Pay
        if (chargeId) {
            r = await put(`/Charge/pay/${chargeId}`);
            logs.push(`缴费: ${r.success ? '成功' : '失败'} - ${r.message}`);
        }

        // 6. Dispense
        if (presId) {
            r = await put(`/Pharmacy/dispense/${presId}`);
            logs.push(`发药: ${r.success ? '成功' : '失败'} - ${r.message}`);
        }

        // 7. Report
        r = await post('/Record/addreport', { patientName: flowName, examItem: '血常规', content: '各项指标正常' });
        logs.push(`录入报告: ${r.success ? '成功' : '失败'} - ${r.message}`);

        // 8. Query
        r = await get(`/Record/getreport/byname?patientName=${encodeURIComponent(flowName)}`);
        logs.push(`查询报告: ${r.success ? '成功' : '失败'} - ${r.message}`);

        container.innerHTML = '<div class="alert alert-success"><h5>流程执行完成</h5><ol>' +
            logs.map(l => `<li>${l}</li>`).join('') +
            '</ol></div>';
    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger">流程执行出错: ${err.message}</div>`;
    }
}

// Initialize
window.addEventListener('DOMContentLoaded', () => {
    loadMedicines();
    loadAllReg();
    loadCharges();
    loadAllRecords();
    loadAllReports();
});

# 医院后台 WebAPI 项目

## 项目概述

本项目是基于 ASP.NET Core 9 开发的医院后台管理系统，包含 WebAPI 后端和 Bootstrap 前端页面，实现了医院核心业务全流程：

- 患者挂号
- 药房管理（药品、处方、发药、库存扣减）
- 收费结算（收费单、缴费、票据、取消）
- 就诊记录
- 检查报告

## 技术栈

- 后端：ASP.NET Core 9 WebAPI
- 数据存储：内存数据（单例服务）
- API 文档：Swagger
- 前端：HTML5 + Bootstrap 5 + JavaScript

## 项目结构

```
HospitalWebAPI/
├── Controllers/          # API 控制器
│   ├── PharmacyController.cs
│   ├── ChargeController.cs
│   ├── PatientController.cs
│   └── RecordController.cs
├── Models/               # 数据模型
│   ├── BaseEntity.cs
│   ├── Medicine.cs
│   ├── Prescription.cs
│   ├── ChargeBill.cs
│   ├── PatientReg.cs
│   ├── MedicalRecord.cs
│   └── Report.cs
├── Services/             # 业务逻辑与内存数据
│   └── HospitalDataService.cs
├── wwwroot/              # 前端页面
│   ├── index.html
│   ├── app.js
│   └── style.css
└── Program.cs
```

## 如何运行

1. 确保已安装 .NET 9 SDK
2. 进入项目目录
3. 执行以下命令启动：

```bash
dotnet run --urls "http://localhost:5000"
```

4. 打开浏览器访问：
   - 前端页面：http://localhost:5000/index.html
   - Swagger API 文档：http://localhost:5000/swagger/index.html

## 核心接口列表

### 患者挂号
- GET /api/Patient/homeindex
- POST /api/Patient/addreg
- GET /api/Patient/myreg?name=姓名
- GET /api/Patient/allreg

### 药房管理
- GET /api/Pharmacy/getallmedicine
- POST /api/Pharmacy/addmedicine
- DELETE /api/Pharmacy/deletemedicine
- POST /api/Pharmacy/addprescription
- GET /api/Pharmacy/getprescription
- PUT /api/Pharmacy/dispense/{id}

### 收费结算
- POST /api/Charge/create/{prescriptionId}
- GET /api/Charge/getall
- PUT /api/Charge/pay/{id}
- PUT /api/Charge/cancel/{id}

### 就诊记录与检查报告
- POST /api/Record/createrecord/{presId}
- GET /api/Record/getrecord/bypres/{presId}
- GET /api/Record/getrecord/bytime
- GET /api/Record/getrecord/all
- POST /api/Record/addreport
- GET /api/Record/getreport/byname
- GET /api/Record/getreport/bytime
- GET /api/Record/getreport/all

## 完整业务流程

```
挂号 → 就诊 → 开处方 → 生成就诊记录 → 收费 → 缴费 → 发药 → 录入报告 → 查询
```

## 异常处理

- 挂号输入不存在科室：返回错误提示
- 处方开药数量大于库存：无法生成处方
- 已结算收费单再次缴费：提示不能重复结算
- 已发药处方重复发药：拦截报错
- 对不存在的处方生成就诊记录：提示处方不存在
- 同一处方重复生成就诊记录：拦截并提示已存在
- 按姓名查询报告无数据：返回暂无报告
- 时间范围无数据：提示当前时间段无记录

## 前端特性

- Bootstrap 5 响应式布局
- 模态弹窗替代原生 alert
- 输入框前端校验（空值、负数、非法字符）
- 表格空数据美化展示
- 新增数据后异步延时刷新列表
- 删除操作二次确认

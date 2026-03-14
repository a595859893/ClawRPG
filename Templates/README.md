# ClawRPG 代码模板

本目录包含用于快速创建新系统/UI的代码模板。

## 使用方法

### 系统模板 (SystemTemplate.cs)

1. 复制 `SystemTemplate.cs` 到 `Scripts/` 目录
2. 重命名文件为 `<系统名>System.cs`
3. 替换占位符:
   - `$SYSTEM_NAME$` → 系统中文名称
   - `$CLASS_NAME$` → 类名(不含System后缀)
   - `$DESCRIPTION$` → 系统描述

### UI模板 (UITemplate.cs)

1. 复制 `UITemplate.cs` 到 `Scripts/` 目录
2. 重命名文件为 `<系统名>UI.cs`
3. 替换占位符:
   - `$UI_NAME$` → UI中文名称
   - `$CLASS_NAME$` → 类名(不含UI后缀)
   - `$DESCRIPTION$` → UI描述

## 示例

创建 "装备强化系统":

1. 创建 `Scripts/EnhancementSystem.cs`
2. 替换 `$CLASS_NAME$` → "Enhancement"
3. 实现具体逻辑

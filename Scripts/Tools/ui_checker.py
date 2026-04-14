#!/usr/bin/env python3
"""
UI Humanization Checker - L1 自检工具
用于检查 UI 文件的基础适人化标准

使用方式:
    python3 scripts/ui_checker.py --ui-name ComboUI --check-level L1
    python3 scripts/ui_checker.py --ui-name AccessibilityUI --check-level L2
    python3 scripts/ui_checker.py --scan-all --check-level L1
"""

import argparse
import json
import os
import re
import sys
from dataclasses import dataclass, field, asdict
from pathlib import Path
from typing import Optional

# 项目根目录
PROJECT_ROOT = Path(__file__).parent.parent.parent
SCRIPTS_UI_DIR = PROJECT_ROOT / "Scripts" / "UI"
SCRIPTS_SYSTEMS_UI_DIR = PROJECT_ROOT / "Scripts" / "Systems"


@dataclass
class CheckResult:
    """单个检查项结果"""
    name: str
    level: str  # L1, L2, L3
    passed: bool
    detail: str
    recommendation: str = ""


@dataclass
class UICheckReport:
    """UI 文件检查报告"""
    ui_name: str
    file_path: str
    level: str
    checks: list = field(default_factory=list)
    passed_count: int = 0
    failed_count: int = 0
    partial_count: int = 0
    
    def add_check(self, check: CheckResult):
        self.checks.append(check)
        if check.passed:
            self.passed_count += 1
        elif "partial" in check.detail.lower():
            self.partial_count += 1
        else:
            self.failed_count += 1
    
    def is_passed(self) -> bool:
        return self.failed_count == 0
    
    def summary(self) -> str:
        status = "✅ PASS" if self.is_passed() else "❌ FAIL"
        return f"{status} ({self.passed_count}✅ {self.partial_count}⚠️ {self.failed_count}❌)"


def find_ui_file(ui_name: str) -> Optional[Path]:
    """查找 UI 文件路径"""
    # 尝试 Scripts/UI/
    ui_file = SCRIPTS_UI_DIR / f"{ui_name}.cs"
    if ui_file.exists():
        return ui_file
    
    # 尝试 Scripts/Systems/ 中的 UI 文件
    systems_ui = SCRIPTS_SYSTEMS_UI_DIR / f"{ui_name}.cs"
    if systems_ui.exists():
        return systems_ui
    
    # 尝试 Scripts/UI/Battle/
    battle_ui = SCRIPTS_UI_DIR / "Battle" / f"{ui_name}.cs"
    if battle_ui.exists():
        return battle_ui
    
    return None


def read_ui_file(file_path: Path) -> str:
    """读取 UI 文件内容"""
    try:
        return file_path.read_text(encoding='utf-8')
    except Exception as e:
        return f"Error reading file: {e}"


def check_font_size(content: str) -> CheckResult:
    """L1: 文字可读性 - 字体大小检查"""
    # 检查是否有 AddThemeFontSizeOverride 或 font_size 设置
    font_size_patterns = [
        r'AddThemeFontSizeOverride\s*\(\s*"[^"]*font_size[^"]*"\s*,\s*(\d+)',
        r'AddThemeFontSizeOverride\s*\(\s*"[^"]*",\s*(\d+)',
        r'FontSize\s*=\s*(\d+)',
        r'Label.*?font_size:\s*(\d+)',
    ]
    
    found_sizes = []
    for pattern in font_size_patterns:
        matches = re.findall(pattern, content)
        found_sizes.extend([int(m) for m in matches])
    
    # 检查是否设置了字体大小
    has_font_setting = bool(found_sizes)
    min_size = min(found_sizes) if found_sizes else 0
    
    # 检查默认值（未设置的情况）
    default_size_patterns = [
        r'new\s+Label\s*\(\s*\).*?;',
        r'var\s+\w+Label\s*=\s*new\s+Label',
    ]
    
    passed = has_font_setting and min_size >= 14
    
    return CheckResult(
        name="文字可读性",
        level="L1",
        passed=passed,
        detail=f"最小字体: {min_size}px" if found_sizes else "未设置自定义字体大小",
        recommendation="字体大小应 ≥14px，确保可读性" if not passed else ""
    )


def check_click_target_size(content: str) -> CheckResult:
    """L1: 点击目标尺寸 - 按钮 ≥44×44px"""
    # 检查 CustomMinimumSize 设置
    min_size_pattern = r'CustomMinimumSize\s*=\s*new\s+Vector2\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)'
    matches = re.findall(min_size_pattern, content)
    
    issues = []
    for w, h in matches:
        w, h = int(w), int(h)
        if w < 44 or h < 44:
            issues.append(f"{w}x{h}")
    
    # 检查 Button 类型的按钮
    button_patterns = [
        r'new\s+Button\s*\(',
        r'new\s+TextureButton\s*\(',
        r'new\s+CheckButton\s*\(',
        r'AddChild\s*\(\s*\w*[Bb]utton',
    ]
    
    buttons_found = any(re.search(p, content) for p in button_patterns)
    
    if not matches:
        detail = "未设置 CustomMinimumSize（按钮可能太小）"
        passed = False
        recommendation = "按钮/可点击元素应 ≥44×44px"
    elif issues:
        detail = f"发现小尺寸点击目标: {', '.join(issues)}"
        passed = False
        recommendation = "所有可点击元素应 ≥44×44px"
    else:
        detail = f"所有点击目标尺寸合规（{len(matches)} 个元素）"
        passed = True
        recommendation = ""
    
    return CheckResult(
        name="点击目标尺寸",
        level="L1",
        passed=passed,
        detail=detail,
        recommendation=recommendation
    )


def check_state_feedback(content: str) -> CheckResult:
    """L1: 状态反馈 - Hover/Active/Disabled/Loading 4状态"""
    patterns = {
        "Hover": [r'MouseEntered', r'Hover', r'_OnMouseEnter', r'OnHover'],
        "Active/Pressed": [r'Pressed', r'Toggled', r'_OnPressed', r'ButtonDown'],
        "Disabled": [r'Disabled', r'_disabled', r'Enabled\s*=\s*false', r'set_mouse_filter\s*\(\s*MouseFilter\.Ignore'],
        "Loading": [r'Loading', r'_isLoading', r'LoadingState', r'SetLoading'],
    }
    
    found_states = {}
    for state, state_patterns in patterns.items():
        for pattern in state_patterns:
            if re.search(pattern, content):
                found_states[state] = True
                break
    
    states_count = len(found_states)
    
    if states_count >= 4:
        passed = True
        detail = f"4状态齐全: {', '.join(found_states.keys())}"
        recommendation = ""
    elif states_count >= 2:
        passed = False
        detail = f"部分状态（{states_count}/4）: {', '.join(found_states.keys())}"
        recommendation = "缺少: " + ", ".join([s for s in patterns.keys() if s not in found_states])
    else:
        passed = False
        detail = f"缺少状态反馈机制（仅 {states_count}/4）"
        recommendation = "需要实现 Hover/Active/Disabled/Loading 状态"
    
    return CheckResult(
        name="状态反馈",
        level="L1",
        passed=passed,
        detail=detail,
        recommendation=recommendation
    )


def check_keyboard_accessibility(content: str) -> CheckResult:
    """L1: 键盘可操作 - 检查快捷键绑定"""
    keyboard_patterns = [
        r'_Input\s*\(',
        r'_UnhandledInput\s*\(',
        r'KeyPress',
        r'KeyDown',
        r'KeyCode',
        r'Keyboard',
        r'Accept',
        r'ui_accept',
        r'ui_left',
        r'ui_right',
        r'TabSwitch',
        r'Hotkey',
        r'Shortcut',
    ]
    
    found = []
    for pattern in keyboard_patterns:
        if re.search(pattern, content, re.IGNORECASE):
            found.append(pattern)
    
    if found:
        passed = True
        detail = f"有键盘处理: {', '.join(found[:3])}"
        recommendation = ""
    else:
        passed = False
        detail = "未发现键盘处理"
        recommendation = "所有功能应支持键盘操作（快捷键/Tab导航）"
    
    return CheckResult(
        name="键盘可操作",
        level="L1",
        passed=passed,
        detail=detail,
        recommendation=recommendation
    )


def check_error_handling(content: str) -> CheckResult:
    """L1: 错误处理 - 有明确错误提示"""
    # 检查是否有错误处理
    error_patterns = {
        "GD.PrintErr": r'GD\.PrintErr',
        "GD.PushError": r'GD\.PushError',
        "Notification": r'PushNotification|ShowNotification',
        "UserFeedback": r'ErrorLabel|ErrorText|_errorLabel|ShowError',
    }
    
    found = []
    for name, pattern in error_patterns.items():
        if re.search(pattern, content):
            found.append(name)
    
    # 检查静默失败的模式
    silent_fail_patterns = [
        r'catch\s*\(\s*\)\s*\{\s*\}',
        r'catch\s*\([^)]+\)\s*\{\s*\/\*?\s*\}',
        r'catch.*?\{\s*//.*?pass.*?\}',
    ]
    
    silent_fails = []
    for pattern in silent_fail_patterns:
        if re.search(pattern, content, re.DOTALL):
            silent_fails.append(pattern[:30])
    
    if silent_fails:
        passed = False
        detail = f"存在静默失败: {len(silent_fails)} 处"
        recommendation = "错误应明确提示，不应静默失败"
    elif found:
        passed = True
        detail = f"有错误处理: {', '.join(found)}"
        recommendation = ""
    else:
        passed = False
        detail = "未发现明确错误处理"
        recommendation = "应有错误提示（GD.PrintErr/Notification/ErrorLabel）"
    
    return CheckResult(
        name="错误处理",
        level="L1",
        passed=passed,
        detail=detail,
        recommendation=recommendation
    )


def check_accessibility_features(content: str) -> CheckResult:
    """L2: 辅助功能 - 色盲模式/高对比度"""
    features = {
        "色盲模式": [r'colorBlind', r'color_blind', r'Deuteranopia', r'Protanopia', r'Tritanopia'],
        "高对比度": [r'highContrast', r'high_contrast', r'contrast'],
        "字体缩放": [r'uiScale', r'ui_scale', r'textSize', r'text_size', r'UIScale'],
        "字幕": [r'subtitle', r'Subtitle', r'_subtitles'],
    }
    
    found = []
    for feature, patterns in features.items():
        for pattern in patterns:
            if re.search(pattern, content, re.IGNORECASE):
                found.append(feature)
                break
    
    if found:
        passed = True
        detail = f"有辅助功能: {', '.join(found)}"
        recommendation = ""
    else:
        passed = False
        detail = "缺少无障碍辅助功能"
        recommendation = "建议添加色盲模式、高对比度、字体缩放等辅助功能"
    
    return CheckResult(
        name="辅助功能",
        level="L2",
        passed=passed,
        detail=detail,
        recommendation=recommendation
    )


def check_humanized_text(content: str) -> CheckResult:
    """L3: 人性化文案 - 检查提示文案"""
    # 检查冷冰冰的系统文案
    cold_patterns = [
        r'"Operation failed"',
        r'"Error occurred"',
        r'"Invalid input"',
        r'"Failed to',
        r'"操作失败"',
        r'"错误"',
        r'"失败"',
    ]
    
    humanized_patterns = [
        r'看来.*?|似乎.*?|也许.*?',
        r'别担心',
        r'没关系',
        r'似乎.*?|好像.*?',
        r'要不再试一次',
        r'这次.*?|下次.*?',
    ]
    
    cold = []
    for pattern in cold_patterns:
        if re.search(pattern, content, re.IGNORECASE):
            cold.append(pattern[:30])
    
    humanized = []
    for pattern in humanized_patterns:
        if re.search(pattern, content):
            humanized.append(pattern[:30])
    
    if cold and not humanized:
        passed = False
        detail = f"使用冷冰冰的文案: {len(cold)} 处"
        recommendation = "提示文案应更有人情味，避免纯技术性错误信息"
    elif humanized:
        passed = True
        detail = f"使用人性化文案: {len(humanized)} 处"
        recommendation = ""
    else:
        passed = True
        detail = "未发现明显问题"
        recommendation = ""
    
    return CheckResult(
        name="人性化文案",
        level="L3",
        passed=passed,
        detail=detail,
        recommendation=recommendation
    )


def check_ui(ui_name: str, level: str = "L1") -> UICheckReport:
    """对单个 UI 文件进行检查"""
    file_path = find_ui_file(ui_name)
    
    if not file_path:
        report = UICheckReport(
            ui_name=ui_name,
            file_path="NOT FOUND",
            level=level
        )
        report.add_check(CheckResult(
            name="文件存在性",
            level=level,
            passed=False,
            detail=f"未找到 {ui_name}.cs",
            recommendation="请确认文件名正确"
        ))
        return report
    
    content = read_ui_file(file_path)
    
    report = UICheckReport(
        ui_name=ui_name,
        file_path=str(file_path.relative_to(PROJECT_ROOT)),
        level=level
    )
    
    # L1 检查（所有层级）
    report.add_check(check_font_size(content))
    report.add_check(check_click_target_size(content))
    report.add_check(check_state_feedback(content))
    report.add_check(check_keyboard_accessibility(content))
    report.add_check(check_error_handling(content))
    
    # L2 检查
    if level in ("L2", "L3"):
        report.add_check(check_accessibility_features(content))
    
    # L3 检查
    if level == "L3":
        report.add_check(check_humanized_text(content))
    
    return report


def scan_all_ui_files(level: str = "L1") -> list:
    """扫描所有 UI 文件"""
    all_files = []
    
    # 扫描 Scripts/UI/
    if SCRIPTS_UI_DIR.exists():
        all_files.extend(list(SCRIPTS_UI_DIR.glob("*.cs")))
        battle_dir = SCRIPTS_UI_DIR / "Battle"
        if battle_dir.exists():
            all_files.extend(list(battle_dir.glob("*.cs")))
    
    # 扫描 Scripts/Systems/ 中的 UI 文件
    if SCRIPTS_SYSTEMS_UI_DIR.exists():
        for f in SCRIPTS_SYSTEMS_UI_DIR.glob("*UI*.cs"):
            all_files.append(f)
    
    reports = []
    for f in sorted(set(all_files)):
        ui_name = f.stem
        report = check_ui(ui_name, level)
        reports.append(report)
    
    return reports


def format_report(report: UICheckReport, verbose: bool = False) -> str:
    """格式化检查报告"""
    lines = []
    lines.append(f"\n{'='*60}")
    lines.append(f"UI Humanization Check Report: {report.ui_name}")
    lines.append(f"File: {report.file_path}")
    lines.append(f"Level: {report.level}")
    lines.append(f"Result: {report.summary()}")
    lines.append('='*60)
    
    if verbose or not report.is_passed():
        for check in report.checks:
            status = "✅" if check.passed else "❌"
            lines.append(f"  {status} [{check.level}] {check.name}")
            lines.append(f"      {check.detail}")
            if check.recommendation:
                lines.append(f"      💡 {check.recommendation}")
    
    return "\n".join(lines)


def format_json(report: UICheckReport) -> str:
    """JSON 格式输出"""
    return json.dumps({
        "ui_name": report.ui_name,
        "file_path": report.file_path,
        "level": report.level,
        "passed": report.is_passed(),
        "passed_count": report.passed_count,
        "partial_count": report.partial_count,
        "failed_count": report.failed_count,
        "checks": [
            {
                "name": c.name,
                "level": c.level,
                "passed": c.passed,
                "detail": c.detail,
                "recommendation": c.recommendation
            }
            for c in report.checks
        ]
    }, ensure_ascii=False, indent=2)


def main():
    parser = argparse.ArgumentParser(
        description="UI Humanization Checker - UI适人化自检工具"
    )
    parser.add_argument(
        "--ui-name",
        help="UI 文件名（不含 .cs 后缀）"
    )
    parser.add_argument(
        "--check-level",
        choices=["L1", "L2", "L3"],
        default="L1",
        help="检查层级（默认 L1）"
    )
    parser.add_argument(
        "--scan-all",
        action="store_true",
        help="扫描所有 UI 文件"
    )
    parser.add_argument(
        "--verbose",
        "-v",
        action="store_true",
        help="显示详细信息"
    )
    parser.add_argument(
        "--json",
        action="store_true",
        help="JSON 格式输出"
    )
    parser.add_argument(
        "--output",
        "-o",
        help="输出到文件"
    )
    
    args = parser.parse_args()
    
    if not args.ui_name and not args.scan_all:
        parser.print_help()
        print("\n示例:")
        print("  python3 scripts/ui_checker.py --ui-name AccessibilityUI --check-level L1")
        print("  python3 scripts/ui_checker.py --ui-name ComboUI --verbose")
        print("  python3 scripts/ui_checker.py --scan-all --check-level L1")
        return 1
    
    if args.scan_all:
        print(f"🔍 扫描所有 UI 文件 (Level {args.check_level})...")
        reports = scan_all_ui_files(args.check_level)
        
        total_passed = sum(1 for r in reports if r.is_passed())
        total_failed = len(reports) - total_passed
        
        output_lines = []
        output_lines.append(f"\n📊 UI Humanization Scan Summary")
        output_lines.append(f"   Total: {len(reports)} UI files")
        output_lines.append(f"   Passed: {total_passed} ✅")
        output_lines.append(f"   Failed: {total_failed} ❌")
        output_lines.append("")
        
        for report in reports:
            output_lines.append(format_report(report, args.verbose))
        
        output = "\n".join(output_lines)
        
    else:
        report = check_ui(args.ui_name, args.check_level)
        output = format_report(report, args.verbose)
        
        if args.json:
            output = format_json(report)
    
    # 输出
    if args.output:
        Path(args.output).write_text(output, encoding='utf-8')
        print(f"✅ 报告已保存到: {args.output}")
    else:
        print(output)
    
    # 返回码：0=通过，1=失败
    if args.scan_all:
        failed_reports = [r for r in reports if not r.is_passed()]
        return 1 if failed_reports else 0
    else:
        return 0 if report.is_passed() else 1


if __name__ == "__main__":
    sys.exit(main())

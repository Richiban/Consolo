import argparse
import csv
import json
import re
from datetime import datetime, timezone
from pathlib import Path


VALUE_PATTERN = re.compile(r"^\s*([\d.]+)\s*([A-Za-zμ]+)?\s*$")
TIME_TO_NS = {"ns": 1, "us": 1_000, "μs": 1_000, "ms": 1_000_000, "s": 1_000_000_000}
BYTES_TO_BYTES = {"B": 1, "KB": 1_000, "MB": 1_000_000, "GB": 1_000_000_000}
TIMING_TOLERANCE = 0.20


def parse_measurement(value, units):
    match = VALUE_PATTERN.match(value)
    if match is None:
        raise ValueError(f"Unrecognized benchmark measurement: {value!r}")

    number = float(match.group(1))
    unit = match.group(2)
    if unit not in units:
        raise ValueError(f"Unrecognized benchmark unit: {unit!r}")

    return number * units[unit]


def read_results(reports_dir):
    benchmarks = {}

    for report_path in sorted(Path(reports_dir).glob("*-report.csv")):
        with report_path.open(newline="", encoding="utf-8-sig") as report_file:
            for row in csv.DictReader(report_file):
                method = row["Method"]
                if not method.startswith("Generated"):
                    continue

                benchmarks[method] = {
                    "mean_ns": parse_measurement(row["Mean"], TIME_TO_NS),
                    "allocated_bytes": parse_measurement(row["Allocated"], BYTES_TO_BYTES),
                }

    if not benchmarks:
        raise ValueError(f"No generated-command results found in {reports_dir}")

    return benchmarks


def compare_results(current, previous):
    comparisons = []
    for method, result in current["benchmarks"].items():
        baseline = previous.get("benchmarks", {}).get(method)
        if baseline is None:
            continue

        comparisons.append(
            {
                "method": method,
                "timing_change": result["mean_ns"] / baseline["mean_ns"] - 1
                if baseline["mean_ns"]
                else None,
                "allocation_change": result["allocated_bytes"] - baseline["allocated_bytes"],
            }
        )

    return comparisons


def write_summary(current, previous, summary_path):
    lines = [
        "## Generated-command benchmarks",
        "",
        f"Commit: `{current['commit']}`",
        "",
    ]

    if previous is None:
        lines.extend(["No previous benchmark result is available yet.", ""])
    else:
        lines.extend(
            [
                f"Compared with `{previous['commit']}`. Timing changes above "
                f"{TIMING_TOLERANCE:.0%} and any allocation increase are reported only; "
                "they do not fail this workflow.",
                "",
                "| Generated benchmark | Mean (ns) | Allocated (bytes) | Comparison |",
                "| --- | ---: | ---: | --- |",
            ]
        )
        comparisons = {
            comparison["method"]: comparison
            for comparison in compare_results(current, previous)
        }
        for method, result in sorted(current["benchmarks"].items()):
            comparison = comparisons.get(method)
            notes = []
            if comparison is None:
                notes.append("no prior result")
            else:
                timing_change = comparison["timing_change"]
                if timing_change is not None and timing_change > TIMING_TOLERANCE:
                    notes.append(f"timing regression +{timing_change:.1%}")
                if comparison["allocation_change"] > 0:
                    notes.append(
                        f"allocation regression +{comparison['allocation_change']:.0f} B"
                    )
                if not notes:
                    notes.append("within tolerance")

            lines.append(
                f"| {method} | {result['mean_ns']:.2f} | "
                f"{result['allocated_bytes']:.0f} | {', '.join(notes)} |"
            )
        lines.append("")

    Path(summary_path).write_text("\n".join(lines), encoding="utf-8")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--reports-dir", required=True)
    parser.add_argument("--commit-sha", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--previous")
    parser.add_argument("--summary", required=True)
    arguments = parser.parse_args()

    current = {
        "commit": arguments.commit_sha,
        "captured_utc": datetime.now(timezone.utc).isoformat(),
        "benchmarks": read_results(arguments.reports_dir),
    }
    Path(arguments.output).write_text(
        json.dumps(current, indent=2, sort_keys=True) + "\n",
        encoding="utf-8",
    )

    previous = None
    if arguments.previous and Path(arguments.previous).is_file():
        previous = json.loads(Path(arguments.previous).read_text(encoding="utf-8"))
    write_summary(current, previous, arguments.summary)


if __name__ == "__main__":
    main()

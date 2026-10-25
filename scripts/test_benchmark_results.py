import csv
import json
import tempfile
import unittest
from pathlib import Path

from benchmark_results import compare_results, main, parse_measurement


class BenchmarkResultsTests(unittest.TestCase):
    def test_parse_measurements_converts_units(self):
        self.assertEqual(parse_measurement("1.25 μs", {"μs": 1_000}), 1_250)
        self.assertEqual(parse_measurement("3 KB", {"KB": 1_000}), 3_000)

    def test_comparison_uses_generated_results_and_raw_allocation_difference(self):
        current = {
            "benchmarks": {
                "GeneratedCommand": {"mean_ns": 125, "allocated_bytes": 24},
            },
        }
        previous = {
            "benchmarks": {
                "GeneratedCommand": {"mean_ns": 100, "allocated_bytes": 20},
            },
        }

        self.assertEqual(
            compare_results(current, previous),
            [
                {
                    "method": "GeneratedCommand",
                    "timing_change": 0.25,
                    "allocation_change": 4,
                },
            ],
        )

    def test_main_writes_commit_keyed_results_and_informational_summary(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            directory = Path(temporary_directory)
            reports = directory / "reports"
            reports.mkdir()
            report_path = reports / "benchmark-report.csv"
            with report_path.open("w", newline="", encoding="utf-8") as report_file:
                writer = csv.DictWriter(
                    report_file,
                    fieldnames=["Method", "Mean", "Allocated"],
                )
                writer.writeheader()
                writer.writerow(
                    {"Method": "GeneratedCommand", "Mean": "125 ns", "Allocated": "24 B"}
                )
                writer.writerow(
                    {"Method": "HandWrittenParser", "Mean": "1 ns", "Allocated": "0 B"}
                )

            previous = directory / "previous.json"
            previous.write_text(
                json.dumps(
                    {
                        "commit": "previous",
                        "benchmarks": {
                            "GeneratedCommand": {
                                "mean_ns": 100,
                                "allocated_bytes": 20,
                            },
                        },
                    }
                ),
                encoding="utf-8",
            )
            output = directory / "results.json"
            summary = directory / "summary.md"

            import sys

            original_argv = sys.argv
            try:
                sys.argv = [
                    "benchmark_results.py",
                    "--reports-dir",
                    str(reports),
                    "--commit-sha",
                    "current",
                    "--output",
                    str(output),
                    "--previous",
                    str(previous),
                    "--summary",
                    str(summary),
                ]
                main()
            finally:
                sys.argv = original_argv

            result = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(result["commit"], "current")
            self.assertEqual(list(result["benchmarks"]), ["GeneratedCommand"])
            summary_text = summary.read_text(encoding="utf-8")
            self.assertIn("timing regression +25.0%", summary_text)
            self.assertIn("allocation regression +4 B", summary_text)
            self.assertIn("do not fail this workflow", summary_text)


if __name__ == "__main__":
    unittest.main()

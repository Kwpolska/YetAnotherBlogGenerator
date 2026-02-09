# YetAnotherBlogGenerator
# Copyright © 2025-2026, Chris Warrick. All rights reserved.
# Licensed under the 3-clause BSD license.

import sys

from pygments_better_html import BetterHtmlFormatter

COLOR_SCHEME = "monokai"
SELECTORS = ["pre.code", ".code .codetable", ".highlight pre"]
WRAPPERS = [".highlight", ".code"]
EXTRA_CSS = """
table.codetable, table.highlighttable { width: 100% }
.codetable td.linenos, td.linenos { text-align: right; width: 3.5em; padding-right: 0.5em; background: rgba(127, 127, 127, 0.2) }
.codetable td.code, td.code { padding-left: 0.5em }
.codetable td.code code { color: unset }
"""


def generate() -> str:
    formatter = BetterHtmlFormatter(style=COLOR_SCHEME)
    pygments_styles: str = formatter.get_style_defs(SELECTORS, WRAPPERS)
    return pygments_styles + EXTRA_CSS


if __name__ == "__main__":
    styles = generate()

    if len(sys.argv) == 1:
        print(styles, end="")
    else:
        with open(sys.argv[1], "w+", encoding="utf-8") as fh:
            fh.write(styles)

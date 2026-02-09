# YetAnotherBlogGenerator
# Copyright © 2025-2026, Chris Warrick. All rights reserved.
# Licensed under the 3-clause BSD license.

import hashlib
import json
import pygments
import re
import sys
import typing
import unicodedata

from pygments.lexers import get_lexer_by_name, get_lexer_for_filename, guess_lexer
from pygments_better_html import BetterHtmlFormatter
from pygments.util import ClassNotFound

if typing.TYPE_CHECKING:
    import pygments.lexer


class Request(typing.TypedDict):
    guid: str
    path: str | None
    language: str | None
    source: str


class Response(typing.TypedDict):
    guid: str
    path: str | None
    success: bool
    html: str


def get_lexer(path: str | None, language: str | None, source: str) -> "pygments.lexer.Lexer":
    if language is not None:
        try:
            return get_lexer_by_name(language)
        except ClassNotFound:
            pass

    if path is not None:
        try:
            return get_lexer_for_filename(path, code=source)
        except ClassNotFound:
            pass

    return guess_lexer(source)


# Taken from Django (BSD-3-Clause license)
# https://github.com/django/django/blob/c72001644fa794b82fa88a7d2ecc20197b01b6f2/django/utils/text.py#L436
def slugify(value: str, allow_unicode: bool = False) -> str:
    """
    Convert to ASCII if 'allow_unicode' is False. Convert spaces or repeated
    dashes to single dashes. Remove characters that aren't alphanumerics,
    underscores, or hyphens. Convert to lowercase. Also strip leading and
    trailing whitespace, dashes, and underscores.
    """
    value = str(value)
    if allow_unicode:
        value = unicodedata.normalize("NFKC", value)
    else:
        value = unicodedata.normalize("NFKD", value).encode("ascii", "ignore").decode("ascii")
    value = re.sub(r"[^\w\s-]", "", value.lower())
    return re.sub(r"[-\s]+", "-", value).strip("-_")


def render(request: Request) -> Response:
    html = ""
    success = True
    try:
        lexer = get_lexer(request["path"], request["language"], request["source"])
        if request["path"]:
            lineanchors = slugify(request["path"])
        else:
            shasum = hashlib.sha1(request["source"].encode("utf-8")).hexdigest()
            lineanchors = f"code_{shasum}"

        formatter = BetterHtmlFormatter(
            lineanchors=lineanchors,
            cssclass="code",
            linenos="table",
            anchorlinenos=True,
            nowrap=False,
        )

        html = pygments.highlight(request["source"], lexer, formatter)
    except Exception as exc:
        success = False
        html = str(exc)

    return {
        "guid": request["guid"],
        "path": request["path"],
        "success": success,
        "html": html,
    }


if __name__ == "__main__":
    requests = json.load(sys.stdin)
    response = [render(request) for request in requests]
    json.dump(response, sys.stdout)

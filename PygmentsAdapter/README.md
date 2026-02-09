# yabg_pygments_adapter

This simple app wraps [Pygments](https://pygments.org/) and [pygments_better_html](https://github.com/Kwpolska/pygments_better_html/) and provides a JSON stdin/stdout interface for them.

This is used by [YetAntotherBlogGenerator (YABG)](https://github.com/Kwpolska/YetAnotherBlogGenerator) to generate code listings.

License: 3-clause BSD.

## Usage

Pipe a JSON list of objects into stdin, receive a JSON list of objects on stdout. The objects look like this:

```python
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
```

The `guid` is used to identify requests and match them up with responses. If `language` is not provided, then `path` will be used to guess it, and if neither is present, Pygments will guess based on the source code.

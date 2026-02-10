# YetAnotherBlogGenerator (YABG)

This is an unoriginally-named static site generator, written in C#.

Its main purpose is to generate my blog at <https://chriswarrick.com/>. The source of that website is available at GitHub: <https://github.com/Kwpolska/website>.

The engine was designed with generating this site in mind, so it probably won’t work for your site.

In addition to the C# code, there is one component written in Python, named `yabg_pygments_adapter`, which is responsible for rendering listings using [Pygments](https://pygments.org/) and [pygments_better_html](https://github.com/Kwpolska/pygments_better_html/).

YABG supports posts and pages (written in Markdown and HTML), galleries, and listings. It can also generate thumbnails, RSS feeds, and sitemaps.

Some design decisions and parts of the output structure were influenced by [Nikola](https://getnikola.com/), which I co-maintain, and to which I am eternally grateful. This is not a re-implementation of Nikola; many features and a lot of customizability was removed. Also, most files are re-rendered from scratch on every run, only thumbnails and code listings are cached (yet the output is quite stable even when running the tool multiple times).

## How to run it

To run YetAnotherBlogGenerator, you will need a computer with .NET 10+ installed (I tested it with Windows and Linux). You will also need a Python virtual environment with `yabg_pygments_adapter` installed into it. For example:

```text
# Linux
$ cd PygmentsAdapter
$ python -m venv .venv
$ .venv/bin/pip install .
```

```text
# Windows
> cd PygmentsAdapter
> py -m venv .venv
> .venv\Scripts\pip install .
```

You will then need to create a `yabg-local.yml` file in the site root with the following content (adjusting for your local environment):

```yaml
outputFolder: "output"
pygmentsAdapterPythonBinary: "C:\\git\\YetAnotherBlogGenerator\\PygmentsAdapter\\.venv\\Scripts\\python.exe"
```

With that, you can run the generator in your site directory:

```text
dotnet run --project C:\git\YetAnotherBlogGenerator\YetAnotherBlogGenerator\YetAnotherBlogGenerator.csproj -c Release
```

(You might also want to run `dotnet publish` and add an alias to the executable, or symlink it into `$PATH` on Linux.)

If you’re on Linux, you might need to install fontconfig. `apt install libfontconfig1` will do it on Debian, `dnf install fontconfig` will do it on Fedora.

## Roadmap

The main goal of this project was parity with Nikola to render my blog. That goal was reached before the code was published. I do not intend to make this any big project, so I probably will not write tests or documentation. That said, feedback and contributions are always welcome.

The code was designed to be testable (since everything is an interface and everything uses dependency injection). Perhaps I could use an LLM to generate tests.

There will probably be tweaks based on my requirements for chriswarrick.com.

## Statement on LLM usage

Roughly 99% of the code in this project was written by a human.

LLM-based autocomplete was used (using the local model included with JetBrains Rider), mostly to save typing.

Some random refactorings and bug troubleshooting sessions were done with the assistance of Claude Haiku 4.5.

## License

Copyright © 2025-2026, Chris Warrick.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions, and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions, and the following disclaimer in the
   documentation and/or other materials provided with the distribution.

3. Neither the name of the author of this software nor the names of
   contributors to this software may be used to endorse or promote
   products derived from this software without specific prior written
   consent.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

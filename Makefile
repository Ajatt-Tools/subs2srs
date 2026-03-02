PREFIX   ?= /usr
DESTDIR  ?=
LIBDIR    = $(DESTDIR)$(PREFIX)/lib/subs2srs
BINDIR    = $(DESTDIR)$(PREFIX)/bin
APPDIR    = $(DESTDIR)$(PREFIX)/share/applications
PROJ      = subs2srs/subs2srs.csproj
PUBLISH   = subs2srs/bin/Release/net10.0/publish

.PHONY: build install uninstall clean

build:
	dotnet publish $(PROJ) -c Release --no-self-contained

install: build
	install -dm755 "$(LIBDIR)"
	cp -r $(PUBLISH)/* "$(LIBDIR)/"
	install -Dm755 dist/subs2srs.sh "$(BINDIR)/subs2srs"
	install -Dm644 dist/subs2srs.desktop "$(APPDIR)/subs2srs.desktop"

uninstall:
	rm -rf "$(DESTDIR)$(PREFIX)/lib/subs2srs"
	rm -f  "$(BINDIR)/subs2srs"
	rm -f  "$(APPDIR)/subs2srs.desktop"

clean:
	dotnet clean $(PROJ) -c Release 2>/dev/null || true
	rm -rf subs2srs/bin subs2srs/obj

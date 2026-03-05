PREFIX   ?= /usr
DESTDIR  ?=
PKGNAME   = subs2srs
LIBDIR    = $(DESTDIR)$(PREFIX)/lib/$(PKGNAME)
BINDIR    = $(DESTDIR)$(PREFIX)/bin
APPDIR    = $(DESTDIR)$(PREFIX)/share/applications
LICDIR    = $(DESTDIR)$(PREFIX)/share/licenses/$(PKGNAME)
PROJ      = subs2srs/subs2srs.csproj
PUBLISH   = subs2srs/bin/Release/net10.0/publish
TESTPROJ  = subs2srs.Tests/subs2srs.Tests.csproj

.PHONY: build test install uninstall clean

build:
	dotnet publish $(PROJ) -c Release --no-self-contained

test:
	dotnet test $(TESTPROJ) -c Release

install: build
	install -dm755 "$(LIBDIR)"
	cp -r $(PUBLISH)/* "$(LIBDIR)/"
	install -Dm755 dist/subs2srs.sh "$(BINDIR)/subs2srs"
	install -Dm644 dist/subs2srs.desktop "$(APPDIR)/subs2srs.desktop"
	install -Dm644 LICENSE "$(LICDIR)/LICENSE"

uninstall:
	rm -rf "$(DESTDIR)$(PREFIX)/lib/$(PKGNAME)"
	rm -f  "$(BINDIR)/subs2srs"
	rm -f  "$(APPDIR)/subs2srs.desktop"
	rm -rf "$(LICDIR)"

clean:
	dotnet clean $(PROJ) -c Release 2>/dev/null || true
	rm -rf subs2srs/bin subs2srs/obj
	rm -rf subs2srs.Tests/bin subs2srs.Tests/obj

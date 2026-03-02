#!/bin/sh
CONFIG_DIR="${XDG_CONFIG_HOME:-$HOME/.config}/subs2srs"
mkdir -p "$CONFIG_DIR"
cd "$CONFIG_DIR"
exec /usr/lib/subs2srs/subs2srs "$@"

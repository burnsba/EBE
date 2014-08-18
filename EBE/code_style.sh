#!/bin/sh

astyle --recursive --style=break --indent=spaces=4 --indent-switches --indent-namespaces --break-blocks --pad-oper --pad-header --delete-empty-lines --break-closing-brackets --max-code-length=150 *.cs
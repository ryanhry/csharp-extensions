#!/bin/sh

#usage: nuget-pack <version>

version="$1"
if [[ -z "$version" ]]; then
	echo 1>&2 "fatal: version required"
	exec print-file-comments "$0"
	exit 1
fi

tag="nuget-$version"

dotnet pack -c Release rm.Extensions/rm.Extensions.csproj \
	-o ../ \
	//p:PackageVersion="$version" \
	//p:PackageReleaseNotes="tag: $tag" \
	&& git tag "$tag" -m "Create nuget tag $tag"

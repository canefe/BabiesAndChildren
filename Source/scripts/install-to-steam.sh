#!/usr/bin/env bash

if [ -z "$RIMWORLD_MODS_DIR" ]; then
	echo "RIMWORLD_MODS_DIR environment variable not set!"
	exit
fi

rm -rf "$RIMWORLD_MODS_DIR/ChildrenAndPregnancy"
mkdir "$RIMWORLD_MODS_DIR/ChildrenAndPregnancy"
cp -R "./"* "$RIMWORLD_MODS_DIR/ChildrenAndPregnancy"
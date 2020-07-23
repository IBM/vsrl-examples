#!/usr/bin/env bash
#
# Copyright (C) 2020 IBM. All Rights Reserved.
#
# See LICENSE.txt file in the root directory
# of this source tree for licensing information.
#

# Check python
command_exists () {
    type "$1" &> /dev/null ;
}
if ! command_exists python3 ; then
    echo "\n Python3 is required. Please install it and run this script again."
    exit
fi

# Install requirements in a new virtual environment
if [ -d "venv" ]; then
    rm -rf venv
fi

python3 -V
python3 -m venv venv

source venv/bin/activate
pip install --upgrade pip
python3 -m pip install --upgrade -r requirements.txt

#!/usr/bin/env bash

# Define ml-agents version
AGENTS_VERSION=0.15.1

# Check python
command_exists () {
    type "$1" &> /dev/null ;
}
if ! command_exists python3 ; then
    echo "\n Python3 is required. Please install it and run this script again."
    exit
fi


# Download/update mlagents
if [ ! -d "ml-agents" ]; then
    git clone https://github.com/Unity-Technologies/ml-agents.git --single-branch --branch $AGENTS_VERSION
else
    cd ml-agents
    git fetch --all --tags
    git checkout tags/$AGENTS_VERSION
    git pull
    cd ..
fi

# Install requirements in a new virtual environment
if [ -d "venv" ]; then
    rm -rf venv
fi

python3 -V
python3 -m venv venv

source venv/bin/activate
python3 -m pip install --upgrade -r requirements.txt
python3 -m pip install --upgrade -e ./ml-agents/ml-agents-envs
python3 -m pip install --upgrade -e ./ml-agents/ml-agents

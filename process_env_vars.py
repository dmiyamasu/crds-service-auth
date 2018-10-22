#!/usr/bin/python3
# -*- coding: utf-8 -*-

# This script will create an Apache .conf file from a template file.
# The template file can contain variables that are replaced with
# values obtained from environment variables that have the same name.
# Example variables: $SERVER_NAME, $WWW_PATH
#
# Usage: python3 processApacheConfig.py <input-template-file> <output-file>

import string
import os
import sys

def processTemplateFile(inputFile, outputFile):
    with open(inputFile, 'r') as templateFile:
        templateStr = string.Template(templateFile.read())
    
    print("Writing to {}".format(outputFile))
    
    with open(outputFile, 'w') as outputConfigFile:
        outputConfigFile.write(templateStr.safe_substitute(os.environ))

def main(args):
    if len(args) < 2:
        print("[ERROR]: insufficient parameters or no parameters provided")
        print("[USAGE]: python3 processApacheConfig.py <input-template-file> <output-file>")
        sys.exit(1)

    inputFile = os.path.abspath(args[0])
    outputFile = os.path.abspath(args[1])

    if not os.path.isfile(inputFile):
        print("[ERROR]: input file does not exist: {}".format(inputFile))
        sys.exit(2)

    processTemplateFile(inputFile, outputFile)
    sys.exit(0)

if __name__ == "__main__":
    main(sys.argv[1:])

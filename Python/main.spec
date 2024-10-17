# main.spec
# -*- mode: python ; coding: utf-8 -*-

import sys
import os
from PyInstaller.utils.hooks import collect_submodules, collect_data_files

block_cipher = None

# Define pathex to include your Python environment path if necessary
pathex = ['F:\\Work\\Unity\\PerfectVisionProject\\perfect-vision-final\\Python', sys.exec_prefix]

# Collect all submodules and data files for pywin32
hiddenimports = collect_submodules('win32com') + collect_submodules('win32com.shell')
datas = collect_data_files('win32com') + [(os.environ['PythonPackagesPath'] + '\\mediapipe\\modules', 'mediapipe\\modules')]

a = Analysis(
    ['main.py'],
    pathex=pathex,
    binaries=[],
    datas=datas,
    hiddenimports=hiddenimports,
    hookspath=[],
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    [],
    exclude_binaries=True,
    name='main',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    console=True,
)

coll = COLLECT(
    exe,
    a.binaries,
    a.zipfiles,
    a.datas,
    strip=False,
    upx=True,
    upx_exclude=[],
    name='main',
)

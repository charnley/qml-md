
import numpy as np
import hashlib
import os

from rdkit import Chem
from rdkit.Chem import rdMolDescriptors
from rdkit.Chem import AllChem

import re


def smiles2hash(smiles):
    molstr = hashlib.md5(smiles.encode('utf-8')).hexdigest()
    return molstr


def get_conformations(smiles, max_conf=20, confs=None):

    m = Chem.MolFromSmiles(smiles)
    m = Chem.AddHs(m)

    rot_bond = rdMolDescriptors.CalcNumRotatableBonds(m)

    if confs is None:
        confs = min(1 + 3*rot_bond, max_conf)

    AllChem.EmbedMultipleConfs(m,
        numConfs=confs,
        useExpTorsionAnglePrefs=True,
        useBasicKnowledge=True)

    return m


def generate_sdf(mol):

    sdf_str = ""

    # writer = Chem.SDWriter(args.filename)
    # for i in range(m.GetNumConformers()):
    #     writer.write(m, i)

    for i in range(mol.GetNumConformers()):
        sdf = Chem.SDWriter.GetText(mol, i)
        sdf_str += sdf

    return sdf_str



def generate_xyz(mol, split=False):

    if split: out = []
    else: out = ""


    for k in range(mol.GetNumConformers()):

        atoms = mol.GetAtoms()
        atoms = list(atoms)

        N = len(atoms)

        xyz_str = ""
        xyz_str += str(N) + "\n"
        xyz_str += "" + "\n"

        for i, atom in enumerate(atoms):

            # name = atom.GetAtomicNum()
            name = atom.GetSymbol()
            pos = mol.GetConformer(k).GetAtomPosition(i)
            x, y, z = pos.x, pos.y, pos.z
            xyz_str += "{0:2s} {1:15.8f} {2:15.8f} {3:15.8f}".format(name, x, y, z)
            xyz_str += "\n"

        if split: out += [xyz_str]
        else: out += xyz_str

    return out



def main():

    import argparse

    description = """
Generate random training data for a QML model.
"""

    parser = argparse.ArgumentParser()
    parser.add_argument('-f', '--filename', type=str, help='', metavar='file')
    parser.add_argument('-o', '--format', default="xyz", type=str, help='XYZ or SDF or MOL or MOL2', metavar='fmt')
    parser.add_argument('-s', '--smiles', action="store", help='SMILES to ', metavar='str')
    parser.add_argument('-n', '--conformations', action="store", help='Number of conformations', metavar='int')
    parser.add_argument('--split', action="store_true", help='Split the file in multiple files. Need {:} in the filename.')
    args = parser.parse_args()

    # Generate random conformations
    m = get_conformations(args.smiles, confs=args.conformations)

    if args.split and "{:}" not in args.filename:
        quit("Need somewhere to put a number")

    if args.format == "xyz":
        out = generate_xyz(m, split=args.split)
    if args.format == "sdf":
        out = generate_sdf(m)

    if not args.split:

        f = open(args.filename, 'w')
        f.write(out)
        f.close()

    else:

        for i, co in enumerate(out):

            f = open(args.filename.format(i), 'w')
            f.write(co)
            f.close()

    return


if __name__ == "__main__":
    main()



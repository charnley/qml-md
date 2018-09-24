
import rmsd
import numpy as np

from qml.data.alchemy import NUCLEAR_CHARGE
from qml.fchl import generate_representation
from qml.fchl import generate_displaced_representations

import wrapper

def main():

    description = """
Based on a list of molecules, train a representation-set and alpha set.
Output the npy files
"""

    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('-f', '--filename', action='store', help='List of molecules', metavar='listfile')
    parser.add_argument('-d', '--dump', action='store', help='Output model in npy format', metavar='file')

    parser.add_argument('--test', action='store_true')
    parser.add_argument('--optimize', action='store_true')

    args = parser.parse_args()


    # Get molecule filenames
    f = open(args.filename, 'r')
    molecules = f.readlines()
    molecules = [mol.strip() for mol in molecules]
    f.close()

    DIRECTORY = args.filename.split("/")
    DIRECTORY = "/".join(DIRECTORY[:-1]) + "/"

    # Init all the rep lists
    list_atoms = []
    list_charges = []
    list_coordinates = []
    list_energies = []
    list_forces = []
    list_rep = []
    list_disp_rep = []
    list_disp_rep5 = []

    # HYPER PARAMETERS
    CUT_DISTANCE = 1e6
    KERNEL_ARGS = {
        "verbose": False,
        "cut_distance": CUT_DISTANCE,
        "kernel": "gaussian",
        "kernel_args": {
            "sigma": [0.64],
        },
    }
    DX = 0.005

    # read coordinates
    for filename in molecules:

        atoms, coordinates = rmsd.get_coordinates_xyz(DIRECTORY + filename + ".xyz")
        nuclear_charges = [NUCLEAR_CHARGE[atom] for atom in atoms]

        f = open(DIRECTORY + filename+".energy", 'r')
        energy = next(f)
        energy = float(energy)

        force = []
        for line in f: force.append(line.split(","))
        force = np.array(force, dtype=float)

        list_atoms.append(atoms)
        list_charges.append(nuclear_charges)
        list_coordinates.append(coordinates)
        list_energies.append(energy)
        list_forces.append(force)


    # Calculate NMAX hyperprameter
    NMAX = [len(x) for x in list_atoms]
    NMAX = np.max(NMAX)

    # Save model parameters
    PARAMETERS = {
        "kernel_args": KERNEL_ARGS,
        "cut_distance": CUT_DISTANCE,
        "max_atoms": NMAX,
        "dx": DX
    }


    # Calculate representations
    for charges, coordinates in zip(list_charges, list_coordinates):

        rep = generate_representation(coordinates, charges, max_size=NMAX, cut_distance=CUT_DISTANCE)
        disp_rep = generate_displaced_representations(coordinates, charges, max_size=NMAX, cut_distance=CUT_DISTANCE, dx=DX)

        list_rep.append(rep)
        list_disp_rep.append(disp_rep)


    list_atoms = np.array(list_atoms)
    list_coordinates = np.array(list_coordinates)
    list_energies = np.array(list_energies)
    list_forces = np.array(list_forces)
    list_rep = np.array(list_rep)
    list_disp_rep = np.array(list_disp_rep)


    # Hack, easy way to normalize energies (same molecule)
    avg = np.sum(list_energies)/len(list_energies)
    list_energies -= avg


    # hatree / bohr to hatree / aangstroem
    list_forces *= 1.0/0.529177249


    # generate train / test views
    view_all = np.array(range(len(molecules)))
    # view_train, view_valid = np.split(view_all, 2)
    view_train = view_all

    # TODO cross-validation of hyper-parameter optimization

    # generate kernel
    kernel_train_energies, kernel_train_deriv = wrapper.get_kernel(
        list_rep[view_train],
        list_rep[view_train],
        list_disp_rep[view_train],
        list_disp_rep[view_train],
        dx=DX,
        kernel_args=KERNEL_ARGS)

    kernel_train_energies = kernel_train_energies[0]
    kernel_train_deriv = kernel_train_deriv[0]

    # generate alphas
    alphas = wrapper.get_alphas(
        kernel_train_energies,
        kernel_train_deriv,
        list_energies[view_train],
        list_forces[view_train])


    # dump the model
    np.save(args.dump + ".alphas", alphas)
    np.save(args.dump + ".representations", list_rep)
    np.save(args.dump + ".displaced_representations", list_disp_rep)
    np.save(args.dump + ".parameters", PARAMETERS)


    # self test
    if args.selftest:
        energy_valid = np.dot(kernel_train_energies.T, alphas)
        force_valid = np.dot(kernel_train_deriv.T, alphas)
        print( mae(list_energies[view_train], energy_valid) < 0.08, "Error in operator test energy" )
        print( mae(list_forces[view_train].flatten(), force_valid) < 0.1, "Error in  operator test force" )


    return

if __name__ == "__main__":
    main()

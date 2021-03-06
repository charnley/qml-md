import sys 
print(sys.path)

import numpy as np
import rmsd

from qml.utils.alchemy import NUCLEAR_CHARGE
from qml.fchl import generate_representation
from qml.fchl import generate_displaced_representations

#wrapper
from scipy.linalg import lstsq
from qml.fchl import get_local_kernels
from qml.fchl import get_atomic_local_kernels
from qml.fchl import get_atomic_local_gradient_kernels


# Default fchl kernel values
KERNEL_ARGS = {
    "verbose": False,
    "cut_distance": 1e6,
    "kernel": "gaussian",
    "kernel_args": {
        "sigma": [0.64],
    },
}

DX = 0.005



def get_kernel(
    representations_a,
    representations_b,
    displaced_representations_a,
    displaced_representations_b,
    kernel_args=KERNEL_ARGS,
    dx=DX):
	
    kernel_property = get_atomic_local_kernels(representations_a, representations_b,   **kernel_args)
    kernel_derivative = get_atomic_local_gradient_kernels(representations_a,  displaced_representations_b, dx=dx, **kernel_args)
	
    return kernel_property, kernel_derivative


def get_alphas(
    kernel_property,
    kernel_derivative,
    energies_list,
    forces_list):

    forces_list = np.concatenate(forces_list)

    Y = np.concatenate((energies_list, forces_list.flatten()))
    C = np.concatenate((kernel_property.T, kernel_derivative.T))
    alphas, residuals, singular_values, rank = lstsq(C, Y, cond=1e-9, lapack_driver="gelsd") 

    return alphas



train_representations = None 
train_displaced_representations = None
train_alphas = None

NMAX = None
CUT_DISTANCE = None
KERNEL_ARGS = None 
DX = None


def read_model(filepath):
	
	global train_representations
	global train_displaced_representations
	global train_alphas
	
	global NMAX
	global CUT_DISTANCE
	global KERNEL_ARGS
	global DX

	# Load model
	PARAMETERS = np.load(filepath + ".parameters.npy")
	train_representations = np.load(filepath + ".representations.npy")
	train_displaced_representations = np.load(filepath + ".displaced_representations.npy")
	train_alphas = np.load(filepath + ".alphas.npy")

	# HYPER PARAMETERS
	CUT_DISTANCE = PARAMETERS.item().get('cut_distance')
	KERNEL_ARGS = PARAMETERS.item().get('kernel_args')
	DX = PARAMETERS.item().get('dx')
	NMAX = PARAMETERS.item().get('max_atoms')

	return NMAX


	
def calculate(charges_mike, coordinates_mike):

	global train_representations
	global train_displaced_representations
	global train_alphas
	
	global NMAX
	global CUT_DISTANCE
	global KERNEL_ARGS
	global DX
	
	print("before")
	
	print("mike charge", charges_mike)
	print("mike coord", coordinates_mike)
	print("mike lr", len(charges_mike))
	print("mike lo", len(coordinates_mike))
	#charges = [NUCLEAR_CHARGE[atom] for atom in atoms]
	#charges = np.array(charges)
	
	N = len(charges_mike)
	
	coordinates = np.zeros(N*3)
	charges = np.zeros(N, dtype=int)
	
	for i, coord in enumerate(coordinates_mike):
		coordinates[i] = coord
	
	for i, charge in enumerate(charges_mike):
		charges[i] = charge
	
	
	coordinates = coordinates.reshape((N,3))
	
	print("charges", charges)
	print("coord", coordinates)
	print("len charge", len(charges))
	print("len coord", len(coordinates))

	
	rep = generate_representation(coordinates, charges, max_size=NMAX, cut_distance=CUT_DISTANCE)
	disp_rep = generate_displaced_representations(coordinates, charges, max_size=NMAX, cut_distance=CUT_DISTANCE, dx=DX)

	list_rep = np.array([rep])
	list_disp_rep = np.array([disp_rep])

	# generate kernel
	kernel_energies, kernel_forces = get_kernel(
        train_representations,
        list_rep,
        train_displaced_representations,
        list_disp_rep)

	kernel_energies = kernel_energies[0]
	kernel_forces = kernel_forces[0]

	print("kernel shape", kernel_forces.shape)
	
	# predict
	energies = np.dot(kernel_energies.T, train_alphas)
	forces = np.dot(kernel_forces.T, train_alphas)
	
	print("after")
	print(energies[0], forces)
	
	return energies[0], forces


	
def main():


    read_model("data/butane")
    atoms, coordinates = rmsd.get_coordinates_xyz("data/butane/butane-1.xyz")

    energy, force = calculate(atoms, coordinates)

    print(energy)
    print(force)
	
    quit()


    description = """
"""

    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('-f', '--filename', action='store', help='List of molecules', metavar='listfile')
    parser.add_argument('-m', '--model', action='store', help='Output model in npy format', metavar='file')
    args = parser.parse_args()


    # Load model
    PARAMETERS = np.load(args.model + ".parameters.npy")
    train_representations = np.load(args.model + ".representations.npy")
    train_displaced_representations = np.load(args.model + ".displaced_representations.npy")
    train_alphas = np.load(args.model + ".alphas.npy")


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
    CUT_DISTANCE = PARAMETERS.item().get('cut_distance')
    KERNEL_ARGS = PARAMETERS.item().get('kernel_args')
    DX = PARAMETERS.item().get('dx')
    NMAX = PARAMETERS.item().get('max_atoms')


    # read coordinates
    for filename in molecules:

        atoms, coordinates = rmsd.get_coordinates_xyz(DIRECTORY + filename + ".xyz")
        charges = [NUCLEAR_CHARGE[atom] for atom in atoms]

        rep = generate_representation(coordinates, charges, max_size=NMAX, cut_distance=CUT_DISTANCE)
        disp_rep = generate_displaced_representations(coordinates, charges, max_size=NMAX, cut_distance=CUT_DISTANCE, dx=DX)

        list_rep.append(rep)
        list_disp_rep.append(disp_rep)

        break


    list_rep = np.array(list_rep)
    list_disp_rep = np.array(list_disp_rep)
	
    # generate kernel
    kernel_energies, kernel_forces = get_kernel(
        train_representations,
        list_rep,
        train_displaced_representations,
        list_disp_rep,
		kernel_args=KERNEL_ARGS,
		dx=DX)

    kernel_energies = kernel_energies[0]
    kernel_forces = kernel_forces[0]


    # predict
    energies = np.dot(kernel_energies.T, train_alphas)
    forces = np.dot(kernel_forces.T, train_alphas)


    print(energies)
    print(forces)

if __name__ == "__main__":
    main()

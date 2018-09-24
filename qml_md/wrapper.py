
import numpy as np

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

    kernel_property = get_atomic_local_kernels(representations_a, representations_b,   **KERNEL_ARGS)
    kernel_derivative = get_atomic_local_gradient_kernels(representations_a,  displaced_representations_b, dx=dx, **KERNEL_ARGS)

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



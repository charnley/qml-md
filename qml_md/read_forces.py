
import re
import numpy as np


def parse_gaussian(filename):

    find_natoms = "NAtoms= "
    find_energy = "SCF Done:  E("
    find_force = "Atomic                   Forces "

    natoms = 0

    f = open(filename, 'r')

    for line in f:

        if find_natoms in line:
            numbers = re.findall(r'\d+', line)
            natoms = int(numbers[0])
            forces = np.zeros((natoms, 3))

        if find_energy in line:
            numbers = re.findall(r'[-]?\d+\.\d*(?:[Ee][-\+]\d+)?', line)
            energy = float(numbers[0])


        if find_force in line:
            line = next(f)
            line = next(f)

            for i in range(natoms):
                line = next(f)
                numbers = re.findall(r'[-]?\d+\.\d*(?:[Ee][-\+]\d+)?', line)
                numbers = [float(x) for x in numbers]
                numbers = np.array(numbers)
                forces[i] = numbers

    return energy, forces


def main():

    import argparse

    parser = argparse.ArgumentParser()
    parser.add_argument('-f', '--filename', type=str, help='', metavar='file')
    args = parser.parse_args()


    energy, forces = parse_gaussian(args.filename)

    print(energy)
    for force in forces:
        print(", ".join([str(x) for x in force]))


    return


if __name__ == "__main__":
    main()

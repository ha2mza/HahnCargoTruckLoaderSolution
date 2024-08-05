using HahnCargoTruckLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HahnCargoTruckLoader.Logic
{
    public class LoadingPlan
    {
        // Dictionary to store the loading instructions for each crate.
        private readonly Dictionary<int, LoadingInstruction> instructions;
        private Truck truck; // Represents the cargo truck.
        private List<Crate> crates; // List of crates to be loaded.
        private bool[,,] cargoSpace; // 3D array to represent the cargo space of the truck.

        // Constructor to initialize the truck and crates.
        public LoadingPlan(Truck truck, List<Crate> crates)
        {
            instructions = new Dictionary<int, LoadingInstruction>(); // Initialize the dictionary.
            this.crates = crates;
            this.truck = truck;
        }

        // Method to generate loading instructions for crates.
        public Dictionary<int, LoadingInstruction> GetLoadingInstructions()
        {
            // Calculate the volumes of the truck and crates.
            int volumeCargoTruck = truck.Width * truck.Height * truck.Length;
            int volumeCrates = this.crates.Sum(crate => crate.Width * crate.Height * crate.Length);

            // Check if any crate is larger than the truck's dimensions.
            bool hasCrateMoreThanWidth = this.crates.Any(crate => crate.Width > truck.Width && crate.Width > truck.Height && crate.Width > truck.Length);
            bool hasCrateMoreThanHeight = this.crates.Any(crate => crate.Height > truck.Height && crate.Height > truck.Width && crate.Height > truck.Length);
            bool hasCrateMoreThanLength = this.crates.Any(crate => crate.Length > truck.Length && crate.Length > truck.Width && crate.Length > truck.Height);

            // Throw exceptions if crates are too large for the truck.
            if (hasCrateMoreThanWidth)
                throw new CrateException("We have a crate with width larger than truck dimensions.");
            if (hasCrateMoreThanHeight)
                throw new CrateException("We have a crate with height larger than truck dimensions.");
            if (hasCrateMoreThanLength)
                throw new CrateException("We have a crate with length larger than truck dimensions.");

            // Check if volumes are valid.
            if (volumeCargoTruck <= 0)
                throw new CargoException("Volume of cargo truck must be greater than 0.");
            if (volumeCrates <= 0)
                throw new CrateException("Volume of crates must be greater than 0.");
            if (volumeCrates > volumeCargoTruck)
                throw new ExceedTruckCargoVolumeException($"The volume of crates ({volumeCrates}) exceeds the volume of cargo ({volumeCargoTruck}).");

            // Initialize the cargo space array.
            this.cargoSpace = new bool[truck.Width, truck.Height, truck.Length];

            // Sort crates by volume in descending order.
            List<Crate> sortedCrates = this.crates.OrderByDescending(crate => crate.Width * crate.Height * crate.Length).ToList();

            // Loop through each crate and try to place it in the cargo space.
            for (int crateIndex = 0; crateIndex < sortedCrates.Count; crateIndex++)
            {
                bool next = false; // Flag to indicate if the crate was placed.

                // Loop through the cargo space.
                for (int i = 0; i < truck.Width; i++)
                {
                    for (int j = 0; j < truck.Height; j++)
                    {
                        for (int k = 0; k < truck.Length; k++)
                        {
                            // Try to place the crate in different orientations.
                            if (CanPlaceCrateWithoutRotation(sortedCrates[crateIndex], i, j, k))
                            {
                                PlaceCrateWithoutRotation(sortedCrates[crateIndex], i, j, k);
                                var loadingInstruction = new LoadingInstruction
                                {
                                    CrateId = sortedCrates[crateIndex].CrateID,
                                    LoadingStepNumber = crateIndex,
                                    TopLeftX = i,
                                    TopLeftY = j,
                                    TurnHorizontal = false,
                                    TurnVertical = false
                                };
                                this.instructions.Add(sortedCrates[crateIndex].CrateID, loadingInstruction);
                                next = true;
                                break;
                            }
                            else if (CanPlaceCrateVertical(sortedCrates[crateIndex], i, j, k))
                            {
                                PlaceCrateVertical(sortedCrates[crateIndex], i, j, k);
                                var loadingInstruction = new LoadingInstruction
                                {
                                    CrateId = sortedCrates[crateIndex].CrateID,
                                    LoadingStepNumber = crateIndex,
                                    TopLeftX = i,
                                    TopLeftY = j,
                                    TurnHorizontal = false,
                                    TurnVertical = true
                                };
                                this.instructions.Add(sortedCrates[crateIndex].CrateID, loadingInstruction);
                                next = true;
                                break;
                            }
                            else if (CanPlaceCrateHorizontal(sortedCrates[crateIndex], i, j, k))
                            {
                                PlaceCrateHorizontal(sortedCrates[crateIndex], i, j, k);
                                var loadingInstruction = new LoadingInstruction
                                {
                                    CrateId = sortedCrates[crateIndex].CrateID,
                                    LoadingStepNumber = crateIndex,
                                    TopLeftX = i,
                                    TopLeftY = j,
                                    TurnHorizontal = true,
                                    TurnVertical = false
                                };
                                this.instructions.Add(sortedCrates[crateIndex].CrateID, loadingInstruction);
                                next = true;
                                break;
                            }
                            else if (CanPlaceCrateBoth(sortedCrates[crateIndex], i, j, k))
                            {
                                PlaceCrateBoth(sortedCrates[crateIndex], i, j, k);
                                var loadingInstruction = new LoadingInstruction
                                {
                                    CrateId = sortedCrates[crateIndex].CrateID,
                                    LoadingStepNumber = crateIndex,
                                    TopLeftX = i,
                                    TopLeftY = j,
                                    TurnHorizontal = true,
                                    TurnVertical = true
                                };
                                this.instructions.Add(sortedCrates[crateIndex].CrateID, loadingInstruction);
                                next = true;
                                break;
                            }
                        }
                        if (next)
                            break;
                    }
                    if (next)
                        break;
                }
            }

            return instructions; // Return the final loading instructions.
        }

        // Method to check if a crate can be placed in the cargo space without rotation.
        bool CanPlaceCrateWithoutRotation(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Width; i++)
                for (int j = 0; j < crate.Height; j++)
                    for (int k = 0; k < crate.Length; k++)
                        try
                        {
                            if (this.cargoSpace[a + i, b + j, c + k])
                                return false;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            return false;
                        }
            return true;
        }

        // Method to check if a crate can be placed  vertically.
        bool CanPlaceCrateVertical(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Height; i++)
                for (int j = 0; j < crate.Width; j++)
                    for (int k = 0; k < crate.Length; k++)
                        try
                        {
                            if (this.cargoSpace[a + i, b + j, c + k])
                                return false;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            return false;
                        }
            return true;
        }

        // Method to check if a crate can be placed horizontally.
        bool CanPlaceCrateHorizontal(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Length; i++)
                for (int j = 0; j < crate.Height; j++)
                    for (int k = 0; k < crate.Width; k++)
                        try
                        {
                            if (this.cargoSpace[a + i, b + j, c + k])
                                return false;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            return false;
                        }
            return true;
        }

        // Method to check if a crate can be placed with both rotations.
        bool CanPlaceCrateBoth(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Width; i++)
                for (int j = 0; j < crate.Length; j++)
                    for (int k = 0; k < crate.Height; k++)
                        try
                        {
                            if (this.cargoSpace[a + i, b + j, c + k])
                                return false;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            return false;
                        }
            return true;
        }

        // Methods to place crates in the cargo space for different orientations.
        void PlaceCrateWithoutRotation(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Width; i++)
                for (int j = 0; j < crate.Height; j++)
                    for (int k = 0; k < crate.Length; k++)
                        this.cargoSpace[a + i, b + j, c + k] = true;
        }

        void PlaceCrateVertical(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Height; i++)
                for (int j = 0; j < crate.Width; j++)
                    for (int k = 0; k < crate.Length; k++)
                        this.cargoSpace[a + i, b + j, c + k] = true;
        }

        void PlaceCrateHorizontal(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Length; i++)
                for (int j = 0; j < crate.Height; j++)
                    for (int k = 0; k < crate.Width; k++)
                        this.cargoSpace[a + i, b + j, c + k] = true;
        }

        void PlaceCrateBoth(Crate crate, int a, int b, int c)
        {
            for (int i = 0; i < crate.Width; i++)
                for (int j = 0; j < crate.Length; j++)
                    for (int k = 0; k < crate.Height; k++)
                        this.cargoSpace[a + i, b + j, c + k] = true;
        }
    }
}

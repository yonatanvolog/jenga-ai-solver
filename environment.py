import socket
import os
import time
from enum import Enum


class CommandType(Enum):
    REMOVE = "remove"
    RESET = "reset"
    TIMESCALE = "timescale"
    ISFALLEN = "isfallen"
    SETSTATICFRICTION = "staticfriction"
    SETDYNAMICFRICTION = "dynamicfriction"
    UNKNOWN = "unknown"


class Environment:
    def __init__(self, host="127.0.0.1", port=25001):
        """Initialize the Environment with the host and port for communication with Unity."""
        self.host = host
        self.port = port

    def send_command(self, command):
        """Send a command to the Unity environment and receive the response."""
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
            sock.connect((self.host, self.port))
            sock.sendall(command.encode("utf-8"))
            response = sock.recv(1024).decode("utf-8")
            return response.strip()

    def reset(self):
        """Reset the Jenga game immediately."""
        response = self.send_command("reset")
        return response

    def step(self, action, wait_seconds=0.5):
        """
        Perform an action to remove a piece from the Jenga tower.

        Parameters:
            action (tuple): A tuple containing the level (int) and color (str - 'y', 'g', 'b') of the piece to remove.
            wait_seconds (float): Time to wait after performing the action before checking if the tower has fallen.

        Returns:
            tuple: A tuple containing the path to the screenshot (str) and a boolean indicating if the tower has fallen.
        """
        level, color = action
        command = f"remove {level} {color}"
        response = self.send_command(command)

        # Wait for the specified time before checking if the tower has fallen
        time.sleep(wait_seconds)

        # Check if the tower has fallen
        is_fallen = self.is_fallen()

        # Retrieve the screenshot after performing the action
        screenshot = self.get_screenshot()
        return screenshot, is_fallen

    def set_timescale(self, timescale):
        """
        Set the timescale of the Jenga game.

        Parameters:
            timescale (float): The timescale to set. For example, a timescale of 2 means the game runs twice as fast as real life.

        Note:
            This value is not saved between runs of the game and defaults to 1. You need to set it each time to ensure it is at the desired value.
        """
        command = f"timescale {timescale}"
        response = self.send_command(command)
        return response

    def set_static_friction(self, static_friction):
        """
        Set the static friction of the Jenga pieces.

        Parameters:
            static_friction (float): The static friction value to set.

        Note:
            This value is not saved between runs of the game and defaults to 1. You need to set it each time to ensure it is at the desired value.
        """
        command = f"staticfriction {static_friction}"
        response = self.send_command(command)
        return response

    def set_dynamic_friction(self, dynamic_friction):
        """
        Set the dynamic friction of the Jenga pieces.

        Parameters:
            dynamic_friction (float): The dynamic friction value to set.

        Note:
            This value is not saved between runs of the game and defaults to 1. You need to set it each time to ensure it is at the desired value.
        """
        command = f"dynamicfriction {dynamic_friction}"
        response = self.send_command(command)
        return response

    def is_fallen(self):
        """
        Check if the Jenga tower has fallen.

        Returns:
            bool: True if the tower has fallen, False otherwise.

        Note:
            If the tower is slowly falling but hasn't tilted enough yet, this method may return False.
            It's advised to call this method after some delay following an action.
        """
        response = self.send_command("isfallen")
        return response.lower() == "true"

    def get_screenshot(self):
        """
        Capture a screenshot of the Jenga tower from a 45-degree angle, showing two sides.

        Returns:
            str: The path to the screenshot file.

        Note:
            This method deletes any previous screenshot in the folder before saving the new one.
        """
        # Directory where screenshots are saved
        screenshot_dir = os.path.join(os.getcwd(), "Assets", "Screenshots")

        # Find the only PNG file in the directory
        png_files = [f for f in os.listdir(screenshot_dir) if
                     f.endswith('.png')]

        if len(png_files) == 1:
            screenshot_path = os.path.join(screenshot_dir, png_files[0])
            return screenshot_path
        else:
            raise FileNotFoundError(
                "Expected one PNG file in the directory, but found {}".format(
                    len(png_files)))


def main():
    env = Environment()

    while True:
        print("\nWhat action would you like to do?")
        print("1: Reset Environment")
        print("2: Perform Action (Remove Piece)")
        print("3: Set Timescale")
        print("4: Set Static Friction")
        print("5: Set Dynamic Friction")
        print("6: Exit")

        choice = input("Enter the number of your choice: ").strip()

        if choice == "1":
            print("Resetting environment...")
            env.reset()
            print("Environment reset.")

        elif choice == "2":
            level = input("Enter the level number: ").strip()
            color = input("Enter the color (y, b, g): ").strip()
            wait_seconds = input(
                "Enter the wait time in seconds (default is 0.5): ").strip()
            if wait_seconds:
                wait_seconds = float(wait_seconds)
            else:
                wait_seconds = 0.5
            action = (level, color)
            print(
                f"Performing action: remove piece at level {level}, color {color}...")
            screenshot, is_fallen = env.step(action, wait_seconds)
            print(f"Action performed. Screenshot saved at: {screenshot}")
            print(f"Has the tower fallen? {'Yes' if is_fallen else 'No'}")

        elif choice == "3":
            timescale = input("Enter the timescale value (e.g., 1.5): ").strip()
            print(f"Setting timescale to {timescale}...")
            env.set_timescale(timescale)
            print("Timescale set.")

        elif choice == "4":
            static_friction = input("Enter the static friction value: ").strip()
            print(f"Setting static friction to {static_friction}...")
            env.set_static_friction(static_friction)
            print("Static friction set.")

        elif choice == "5":
            dynamic_friction = input(
                "Enter the dynamic friction value: ").strip()
            print(f"Setting dynamic friction to {dynamic_friction}...")
            env.set_dynamic_friction(dynamic_friction)
            print("Dynamic friction set.")

        elif choice == "6":
            print("Exiting...")
            break

        else:
            print("Invalid choice, please try again.")


if __name__ == "__main__":
    main()

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;


namespace WpfAppfinal
{

    // Recipe class
    public class Recipe
    {
        public string RecipeName { get; set; }
        public int Calories { get; set; }
        public string FoodGroup { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public List<string> Steps { get; set; }
    }

    // Ingredient class
    public class Ingredient
    {
        public string Name { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
    }

    // RecipeManagement class
    public class RecipeManagement
    {
        private List<Recipe> recipes;

        public RecipeManagement()
        {
            recipes = new List<Recipe>();
        }

        public void AddRecipe(Recipe recipe)
        {
            recipes.Add(recipe);
        }

        public List<Recipe> GetRecipeList()
        {
            return recipes.OrderBy(r => r.RecipeName).ToList();
        }

        public Recipe GetRecipeByName(string recipeName)
        {
            return recipes.FirstOrDefault(r => r.RecipeName.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
        }

        public void ScaleRecipe(string recipeName, double scaleFactor)
        {
            var recipe = GetRecipeByName(recipeName);
            if (recipe != null)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    ingredient.Quantity *= scaleFactor;
                }
            }
        }

        public void ResetQuantities(string recipeName)
        {
            var recipe = GetRecipeByName(recipeName);
            if (recipe != null)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    // Assuming the original quantities are stored somewhere
                    ingredient.Quantity = GetOriginalQuantity(ingredient.Name);
                }
            }
        }

        public List<Recipe> FilterByIngredient(string ingredientName)
        {
            return recipes.Where(r => r.Ingredients.Any(i => i.Name.Equals(ingredientName, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        public List<Recipe> FilterByFoodGroup(string foodGroup)
        {
            return recipes.Where(r => r.FoodGroup.Equals(foodGroup, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Recipe> FilterByCalories(int maxCalories)
        {
            return recipes.Where(r => r.Calories <= maxCalories).ToList();
        }

        // Helper method to get original ingredient quantity
        private double GetOriginalQuantity(string ingredientName)
        {
            // Implement your logic to retrieve the original quantity from storage/database
            return 0.0; // Placeholder value, replace with your implementation
        }
    }




    // MainWindow class (UI)
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private RecipeManagement recipeManagement;
        private List<Recipe> displayedRecipes;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            recipeManagement = new RecipeManagement();
            displayedRecipes = new List<Recipe>();



            UpdateDisplayedRecipes();
        }

        private void ScaleFactorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedScaleFactor = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();

            // Perform the scaling logic based on the selectedScaleFactor
            // ...
        }



        private void AddRecipeButton_Click(object sender, RoutedEventArgs e)
        {


            // Create a new recipe from the input fields
            var recipe = new Recipe
            {
                RecipeName = RecipeNameTextBox.Text,
                Calories = int.Parse(CaloriesTextBox.Text),
                FoodGroup = FoodGroupTextBox.Text,
                Ingredients = new List<Ingredient>(),
                Steps = new List<string>()
            };

            // Add ingredients to the recipe
            foreach (var ingredientText in IngredientsTextBox.Text.Split('\n'))
            {
                var parts = ingredientText.Trim().Split(' ');
                if (parts.Length == 3)
                {
                    var ingredient = new Ingredient
                    {
                        Name = parts[0],
                        Quantity = double.Parse(parts[1]),
                        Unit = parts[2]
                    };
                    recipe.Ingredients.Add(ingredient);
                }
            }

            // Add steps to the recipe
            foreach (var stepText in StepsTextBox.Text.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(stepText))
                {
                    recipe.Steps.Add(stepText.Trim());
                }
            }

            // Add the recipe to the management class
            recipeManagement.AddRecipe(recipe);

            // Clear input fields
            RecipeNameTextBox.Clear();
            CaloriesTextBox.Clear();
            FoodGroupTextBox.Clear();
            IngredientsTextBox.Clear();
            StepsTextBox.Clear();

            UpdateDisplayedRecipes();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDisplayedRecipes();
        }

        private void ScaleRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItem is Recipe selectedRecipe && ScaleFactorComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var scaleFactor = double.Parse(selectedItem.Content.ToString());
                recipeManagement.ScaleRecipe(selectedRecipe.RecipeName, scaleFactor);
                UpdateDisplayedRecipes();
            }
        }

        private void ResetQuantitiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItem is Recipe selectedRecipe)
            {
                recipeManagement.ResetQuantities(selectedRecipe.RecipeName);
                UpdateDisplayedRecipes();
            }
        }

        private void DisplayRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItem is Recipe selectedRecipe)
            {
                var recipeDetails = $"Name: {selectedRecipe.RecipeName}\n" +
                                    $"Calories: {selectedRecipe.Calories}\n" +
                                    $"Food Group: {selectedRecipe.FoodGroup}\n" +
                                    "\nIngredients:\n";

                foreach (var ingredient in selectedRecipe.Ingredients)
                {
                    recipeDetails += $"{ingredient.Name} - {ingredient.Quantity} {ingredient.Unit}\n";
                }

                recipeDetails += "\nSteps:\n";

                foreach (var step in selectedRecipe.Steps)
                {
                    recipeDetails += $"{step}\n";
                }

                MessageBox.Show(recipeDetails, "Recipe Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearDataButton_Click(object sender, RoutedEventArgs e)
        {
            recipeManagement = new RecipeManagement();
            UpdateDisplayedRecipes();
        }

        private void UpdateDisplayedRecipes()
        {
            displayedRecipes.Clear();

            var filterText = FilterTextBox.Text.ToLower();

            foreach (var recipe in recipeManagement.GetRecipeList())
            {
                if (string.IsNullOrEmpty(filterText) ||
                    recipe.RecipeName.ToLower().Contains(filterText) ||
                    recipe.FoodGroup.ToLower().Contains(filterText) ||
                    recipe.Ingredients.Any(i => i.Name.ToLower().Contains(filterText)))
                {
                    displayedRecipes.Add(recipe);
                }
            }

            RecipeListBox.ItemsSource = displayedRecipes;
            RecipeListBox.Items.Refresh();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterTextBox.Clear();
            UpdateDisplayedRecipes();
        }
    }
}

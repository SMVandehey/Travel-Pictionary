# Project Design - Travel Pictionary

## Overview

Travel Pictionary is an offline, pass-and-play version of Pictionary. It is a family-friendly game designed for long car rides and other travel. Two to four players take turns drawing a picture from a prompt while the other players guess what is being drawn.

## Target Platforms

The application will remain cross-platform and will be implemented with .NET MAUI. Android phones and tablets are the first development and validation targets. iOS support will be developed at a later date using the same shared application architecture wherever possible.

The application must not depend on platform-specific online services. Platform-specific code should be isolated behind MAUI abstractions or small platform implementations when required.

## Technical Specifications

- The project will be written with .NET MAUI and target the existing .NET 10 project configuration.
- The game will run fully offline with no online capabilities, account system, or server dependency.
- The drawing canvas will use the .NET MAUI Community Toolkit `DrawingView` control.
- The prompt data will be loaded from `Resources/prompt-list.txt`, which contains a JSON flat array of strings.
- Prompts must be selected randomly without repetition within a game. A game must track the prompts already used and exclude them from subsequent selections.
- Prompt loading and parsing errors must be handled without crashing the game. The implementation should provide a user-facing error or safe fallback if the prompt asset is missing or invalid.

## Application Architecture

The implementation should separate game rules from page presentation:

- A game/session model will contain the players, scores, current round, current player, used prompts, and game status.
- A game service or coordinator will own turn sequencing, round progression, prompt selection, scoring, and winner calculation.
- View models will expose bindable state and commands for the Home, Player Select, turn transition, word prompt, drawing, and Summary screens.
- The prompt repository will read and deserialize the local JSON asset through MAUI asset APIs.
- Navigation and timed transitions will be coordinated so that a timer can only advance the active phase once, even if a page is leaving or the application is interrupted.
- All timers and event subscriptions must be stopped or disposed when their screen is left.

## Functional Specifications

### General game rules

- There will be exactly three rounds.
- Each player's score starts at zero.
- When the game starts, the first player will be selected randomly.
- Players take turns in player order, wrapping from the last player to the first.
- A round ends after every player has completed one turn.
- The game ends after the third round and displays the Summary screen.
- Each prompt may be used at most once during a game. Prompt reuse is allowed in a later game.

### Home screen

- Display the title `Alaric's Drawing Duel`.
- Display a `New Game` button.
- Selecting `New Game` plays a transition animation and navigates to Player Select.

### Player Select screen

- Display the question `How many players are playing?`.
- Provide choices for 2, 3, or 4 players.
- After a player count is selected, create the corresponding name-entry controls.
- Preserve the previous game's player names as initial values when starting a new game from Summary.
- The `Ready` button will be disabled until every required name is nonempty after trimming whitespace.
- Player names should be validated for a usable length and should not be accepted as whitespace-only values.
- Selecting `Ready` creates a new game with zeroed scores, an empty used-prompt set, and a randomly selected starting player, then navigates to the first turn.

### Turn transition screen

- Display `{name}'s turn! Pass them the canvas!`.
- Display the transition for five seconds.
- Prevent the player from accidentally advancing the turn more than once.
- After the transition completes, navigate to the Word Prompt screen.

### Word Prompt screen

- Select one unused prompt at random from the flat prompt list.
- Record the selected prompt as used for the current game before showing the drawing phase.
- Display `Your word is {word}!`.
- Display a five-second countdown.
- At the end of the countdown, navigate to the Drawing screen.
- Ensure the prompt remains hidden from the other players as much as practical for a pass-and-play game.

### Drawing screen

- Display an editable `DrawingView` in the center of the screen.
- Display a 60-second countdown.
- Display one `{name} got it!` button for each player other than the drawing player.
- Provide ten predefined standard colors for drawing.
- Keep line width fixed and do not expose a line-width editor.
- When any `got it` button is selected:
  - Immediately mark the turn as completed.
  - Award one point to the player who selected the button.
  - Award two points to the drawing player.
  - Disable all `got it` buttons immediately to prevent duplicate scoring during the screen transition.
  - Stop the drawing timer and navigate to the next turn or Summary screen.
- Only the first successful `got it` action may score. Command guards and disabled-button state must both be used so multiple taps cannot produce multiple scores.
- If the 60-second timer expires first, end the turn without awarding points and navigate to the next turn or Summary screen.
- Timer expiry and a `got it` action must be mutually exclusive; whichever completion is processed first owns the turn.

### Summary screen

- List every player's final score.
- Determine all players whose scores equal the highest score.
- Display one winner message for a single winner and a multiple-winner message when there is a tie, such as `{Name 1} and {Name 2} tie!`.
- Provide a `New Game` button that returns to Player Select.
- Prefill the previous player names on the next Player Select screen.

## Styling and Assets

The visual style is inspired by hand-drawn middle-grade comics: simple defined outlines, solid bright colors, playful irregular shapes, and a paper/sketchbook feel. The attached mockups are visual references only; they are not source artwork and should not be copied as original application assets.

- Recreate the layouts with MAUI XAML controls, shapes, borders, colors, animations, and locally supplied assets.
- Use a warm paper-like background, strong black outlines, colorful buttons, hand-drawn-looking spacing, and large readable typography.
- Prefer XAML controls and local vector or raster assets for illustrations and decoration.
- If a suitable comic-style font is not already available, select and include an appropriately licensed font that permits redistribution in the application. Keep a readable fallback font for accessibility and platform compatibility.
- Use the free Font Awesome version only where an icon cannot be reasonably represented with MAUI/XAML controls or a local asset. Verify that the selected Font Awesome package and individual icons have licensing compatible with distribution.
- Design layouts responsively for Android phones and tablets, including portrait and landscape orientations where practical. Avoid relying on fixed pixel coordinates from the mockups.
- Ensure text contrast, touch targets, focus behavior, and semantic descriptions remain usable despite the decorative styling.

## Testing and Validation Plan

- Unit test prompt deserialization, empty/invalid prompt lists, random selection, and no-repeat behavior within a game.
- Unit test player count validation, name validation, turn order, round progression, score updates, timeout behavior, and winner/tie calculation.
- Test that duplicate `got it` actions cannot score more than once, including rapid repeated taps during navigation.
- Test the three timed phases using an injectable clock or timer abstraction rather than relying exclusively on real-time delays.
- Validate the primary flows manually on Android phone and tablet form factors: new game, each player count, all three rounds, timeout, successful guess, tied scores, and starting another game with prefilled names.
- Perform a later iOS validation pass after the Android-first implementation is stable.

## Resolved Design Decisions

- Prompt source: `Resources/prompt-list.txt` in JSON format.
- Prompt organization: one flat list.
- Prompt uniqueness: no repeated prompt within a game.
- Platform strategy: cross-platform .NET MAUI, with Android developed first and iOS later.
- Successful guess behavior: the first `got it` selection immediately ends the turn; all other buttons are disabled to prevent multiple selections.
- Ties: the Summary screen displays multiple winners when players share the highest score.
- Mockups: styling references only; no original artwork is available.
- Fonts and icons: use appropriately licensed local assets, with Font Awesome Free available for icons that cannot be reproduced with XAML controls.

## Remaining Implementation Considerations

- Select the final comic-style font and confirm its redistribution license before adding it to the project.
- Decide which decorative illustrations can be created as simple XAML shapes and which require locally created or licensed assets.
- Confirm the exact wording and formatting for the multi-winner summary message during UI implementation.

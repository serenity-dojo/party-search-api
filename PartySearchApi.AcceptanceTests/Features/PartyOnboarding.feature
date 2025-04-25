Feature: Party Onboarding

Rule: API clients can onboard a new party via the /party endpoint
	Example: Carrie onboards a new party with valid party details
		Given Chuck has onboarded a party with the following details:
			| Party ID  | Name        | Type         | Sanctions Status | Match Score |
			| P12345678 | Axe Capital | Organization | Approved         | 0.95        |
		When Chuck searches for "Axe Capital"
		Then the search results should contain exactly:
			| Party ID  | Name        | Type         | Sanctions Status | Match Score |
			| P12345678 | Axe Capital | Organization | Approved         | 0.95        |



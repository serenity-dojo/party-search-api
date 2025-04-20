@allure.label.epic:Sanctions
@allure.label.feature:PartySearch
Feature: Search for a sanctioned party by name or ID
  
  @allure.label.story:SearchByPartyName
  Rule: Searches should return the correct parties based on full or partial name

    Example: Search by full name returns an exact match
      # Connie is a compliance officer responsible for reviewing third-party relationships.
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
        | P87654321 | Axel Accounting  | Organization | Pending Review   |         70% |
        | P87654329 | Axe Capital      | Organization | Escalated        |         85% |
      When Connie searches for "Acme Corporation"
      Then the search results should contain exactly:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |

    Example: Search by partial name returns all matching parties
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
        | P87654321 | Acme Inc.        | Organization | Pending Review   |         65% |
        | P87654329 | Axe Capital      | Organization | Escalated        |         85% |
      When Connie searches for "Acme"
      Then the search results should contain exactly:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
        | P87654321 | Acme Inc.        | Organization | Pending Review   |         65% |

  @allure.label.story:SearchByPartyId
  Rule: Searches should return the correct parties based on ID

    Example: Search by ID returns the correct party
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
        | P87654329 | Axe Capital      | Organization | Escalated        |         85% |
      When Connie searches for "P12345678"
      Then the search results should contain exactly:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |

    Example: Search by partial ID returns the matching party
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
        | P12345329 | Axe Capital      | Organization | Escalated        |         85% |
        | P87654321 | Axel Accounting  | Organization | Pending Review   |         70% |
      When Connie searches for "P12345"
      Then the search results should contain exactly:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
        | P12345329 | Axe Capital      | Organization | Escalated        |         85% |

    Example: Search for an unknown ID returns no results
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
      When Connie searches for "XYZ"
      Then the search results should be empty

  @allure.label.story:SearchByPartyName
  Rule: Searches should be case-insensitive

    Example: The one where Connie searches for "acme corporation"
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
      When Connie searches for "acme corporation"
      Then the search results should contain exactly:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |

  @allure.label.story:SortingSearchResults
  Rule: Search results are ordered in alphabetical order

    Example: Search results are ordered by name
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | Acme Corporation | Organization | Approved         |         95% |
        | P87654321 | Axel Accounting  | Organization | Pending Review   |         70% |
        | P87654329 | Axe Capital      | Organization | Escalated        |         85% |
      When Connie searches for "Axe"
      Then the search results should contain exactly:
        | Party ID  | Name            | Type         | Sanctions Status | Match Score |
        | P87654329 | Axe Capital     | Organization | Escalated        |         85% |
        | P87654321 | Axel Accounting | Organization | Pending Review   |         70% |

  @allure.label.story:FilterSearchResults
  Rule: Searches can be filtered by Type

    Example: Connie searches for organisations named Smith
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | John Smith       | Individual   | Approved         |         90% |
        | P87654321 | Jane Smith       | Individual   | Pending Review   |         85% |
        | P87654329 | Smith Johnson    | Organization | Escalated        |         80% |
        | P87654339 | Sarah-Jane Smith | Individual   | False Positive   |         80% |
      When Connie searches for "Smith" with the following filters:
        | Filter | Value        |
        | Type   | Organization |
      Then the search results should contain exactly:
        | Party ID  | Name          | Type         | Sanctions Status | Match Score |
        | P87654329 | Smith Johnson | Organization | Escalated        |         80% |

    Example: Connie searches for individuals named Smith
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | John Smith       | Individual   | Approved         |         90% |
        | P87654321 | Jane Smith       | Individual   | Pending Review   |         85% |
        | P87654329 | Smith Johnson    | Organization | Escalated        |         80% |
        | P87654339 | Sarah-Jane Smith | Individual   | False Positive   |         80% |
      When Connie searches for "Smith" with the following filters:
        | Filter | Value      |
        | Type   | Individual |
      Then the search results should contain exactly:
        | Party ID  | Name             | Type       | Sanctions Status | Match Score |
        | P12345678 | John Smith       | Individual | Approved         |         90% |
        | P87654321 | Jane Smith       | Individual | Pending Review   |         85% |
        | P87654339 | Sarah-Jane Smith | Individual | False Positive   |         80% |

  @allure.label.story:FilterSearchResults
  Rule: Search results can be filtered by Sanction Status

    Example: Larry filters his search results to only show parties that are Pending Review
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | John Smith       | Individual   | Approved         |         90% |
        | P87654321 | Jane Smith       | Individual   | Pending Review   |         85% |
        | P87654329 | Smith Johnson    | Organization | Escalated        |         80% |
        | P87654339 | Sarah-Jane Smith | Individual   | Pending Review   |         80% |
      When Larry searches for "Smith" with the following filters:
        | Filter | Value          |
        | Status | Pending Review |
      Then the search results should contain exactly:
        | Party ID  | Name             | Type       | Sanctions Status | Match Score |
        | P87654321 | Jane Smith       | Individual | Pending Review   |         85% |
        | P87654339 | Sarah-Jane Smith | Individual | Pending Review   |         80% |

    Example: Connie filters her search results to only Individuals that are Confirmed Matches
      Given the following parties exist:
        | Party ID  | Name             | Type         | Sanctions Status | Match Score |
        | P12345678 | John Smith       | Individual   | Confirmed Match  |         90% |
        | P87654321 | Jane Smith       | Individual   | Pending Review   |         85% |
        | P87654329 | Smith Johnson    | Organization | Confirmed Match  |         99% |
        | P87654339 | Sarah-Jane Smith | Individual   | False Positive   |         80% |
      When Connie searches for "Smith" with the following filters:
        | Filter          | Value           |
        | Type            | Individual      |
        | Sanction Status | Confirmed Match |
      Then the search results should contain exactly:
        | Party ID  | Name          | Type         | Sanctions Status | Match Score |
        | P12345678 | John Smith    | Individual   | Confirmed Match  |         90% |

  @allure.label.story:FilterSearchResults
  Rule: Large result sets are paginated for easier navigation

    Example: Search results are paginated
      Given 100 parties exist with a name containing "Smith"
      When Connie searches for "Smith" with the following parameters:
        | Page     |  pageSize |
        | 1        | 10        |
      Then the parties returned should be items 1-10 of the complete result set
      And the response should include pagination metadata:
		| totalResults | totalPages | currentPage | pageSize |
		| 100          | 10         | 1           | 10       |

    Example: Fetching the results for a different page
      Given 95 parties exist with a name containing "Smith"
      When Connie searches for "Smith" with the following parameters:
        | Page     |  pageSize |
        | 2        | 10        |
      Then the parties returned should be items 11-20 of the complete result set
      And the response should include pagination metadata:
		| totalResults | totalPages | currentPage | pageSize |
		| 95           | 10         | 2           | 10       |

import getpass

def conditions_met(question, responses):
    return all(condition(responses) for condition in question.get("Conditions", []))

def ask_questions(questions, responses):
    for question in questions:
        if not conditions_met(question, responses):
            continue
        
        ask_question(question, responses)

def input_request(question, default, valid_values):
    if(question["Type"] == "password"):
        response = getpass.getpass(question["Question"] + " ")
    else:
        options = f" (Options: {', '.join(valid_values)})" if valid_values else ""
        response = input(question["Question"] + f"{options} [{default}] " if default else question["Question"] + " ")
    
    if not response and default:
        response = default

    if question["Type"] == "boolean":
        return response.lower() in ["yes", "y", "true", "t", "1"]
    else:
        return response

def ask_question(question, responses):
    valid_values = question.get("ValidValues", [])
    default = question.get("Default", "")
    
    response = input_request(question, default, valid_values)
    valid_selection = False

    while not valid_selection:
        valid_selection = not valid_values or response in valid_values
        if not valid_selection:
            print(f"Invalid input. Please choose from the following options: {', '.join(valid_values)}")
            response = input_request(question, default, valid_values)

    responses[question['Name']] = {"input": response}
    
    # Handle sub-questions if conditions are met
    if "SubQuestionSets" in question:
        for sub_question_set in question["SubQuestionSets"]:
            # Check if conditions for this sub-question set are met
            if conditions_met(sub_question_set, responses):
                responses[sub_question_set["Name"]] = {}
                # Recursively ask sub-questions
                ask_questions(sub_question_set["Questions"], responses)

            # Perform any actions associated with this sub-question set
            if "Action" in sub_question_set:
                responses[sub_question_set["Name"]]["action_result"] = sub_question_set["Action"](responses)
    
    # Perform any actions associated with the main question
    if "Action" in question:
        responses[question["Name"]]["action_result"] = question["Action"](responses)

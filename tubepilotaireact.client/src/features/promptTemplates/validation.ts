import type { CreatePromptTemplatePayload, PromptTemplatePayload } from '../../types/promptTemplate';

export function validatePromptTemplate(payload: PromptTemplatePayload | CreatePromptTemplatePayload) {
  const errors: string[] = [];

  addLengthError(errors, 'Name', payload.name, 3, 120, true);
  addLengthError(errors, 'Category', payload.category, 2, 80, true);
  addLengthError(errors, 'Description', payload.description, 0, 500, false);
  addLengthError(errors, 'Template text', payload.templateText, 10, 8000, true);
  addLengthError(errors, 'System message', payload.systemMessage, 0, 2000, false);

  if ('createdBy' in payload) {
    addLengthError(errors, 'Created by', payload.createdBy, 2, 120, true);
  }

  const normalizedVariables = payload.variables.map((variable) => variable.trim()).filter(Boolean);
  const duplicateVariables = normalizedVariables.filter(
    (variable, index) =>
      normalizedVariables.findIndex((candidate) => candidate.toLowerCase() === variable.toLowerCase()) !== index,
  );

  if (duplicateVariables.length > 0) {
    errors.push(`Variables must be unique: ${Array.from(new Set(duplicateVariables)).join(', ')}.`);
  }

  for (const variable of payload.variables) {
    if (!variable.trim()) {
      errors.push('Variables cannot include blank values.');
    }

    if (variable.length > 80) {
      errors.push(`Variable "${variable}" cannot be longer than 80 characters.`);
    }
  }

  return errors;
}

function addLengthError(
  errors: string[],
  field: string,
  value: string | null | undefined,
  min: number,
  max: number,
  required: boolean,
) {
  const trimmed = value?.trim() ?? '';

  if (!trimmed) {
    if (required) {
      errors.push(`${field} is required.`);
    }

    return;
  }

  if (trimmed.length < min) {
    errors.push(`${field} must be at least ${min} characters.`);
  }

  if (trimmed.length > max) {
    errors.push(`${field} cannot be longer than ${max} characters.`);
  }
}
